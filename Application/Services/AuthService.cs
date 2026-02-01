using AutoMapper;
using backend.Application.DTOs.Auth.Requests;
using backend.Application.DTOs.Auth.Responses;
using backend.Application.DTOs.User.Responses;
using backend.Application.Interfaces;
using backend.Domain.User.Interfaces;

namespace backend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    public AuthService(
        IUserRepository userRepository, 
        IJwtService jwtService, 
        IMapper mapper, 
        IPermissionService permissionService,
        ILocalizationService localizationService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _mapper = mapper;
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException(_localizationService.GetString("UserAlreadyExists"));
        }

        // Create new user using AutoMapper
        var user = _mapper.Map<Domain.User.Entities.User>(request);

        await _userRepository.CreateAsync(user, request.Password);

        // Assign role
        var role = string.IsNullOrEmpty(request.Role) ? "User" : request.Role;
        
        if (!await _userRepository.RoleExistsAsync(role))
        {
            await _userRepository.CreateRoleAsync(role);
        }

        await _userRepository.AddToRoleAsync(user, role);

        var roles = await _userRepository.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        var token = _jwtService.GenerateToken(user, roles, permissions.ToList());
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Tìm user theo username hoặc email
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null)
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("LoginFailed"));
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("UserAccountInactive"));
        }

        var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("LoginFailed"));
        }

        var roles = await _userRepository.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        var token = _jwtService.GenerateToken(user, roles, permissions.ToList());
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("InvalidToken"));
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("InvalidToken"));
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(_localizationService.GetString("UserNotFound"));
        }

        var roles = await _userRepository.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        var newToken = _jwtService.GenerateToken(user, roles, permissions.ToList());
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }

    public async Task<UserResponse> GetCurrentUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("UserNotFound"));
        }

        var roles = await _userRepository.GetRolesAsync(user);
        var userResponse = _mapper.Map<UserResponse>(user);
        userResponse.Roles = roles;
        
        return userResponse;
    }

    public async Task<string> UpdateLanguageAsync(string userId, string language)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("UserNotFound"));
        }

        // Validate language code từ database sẽ được thực hiện ở controller/service khác
        // Ở đây chỉ cập nhật language cho user
        await _userRepository.UpdateLanguageAsync(user, language);
        return language;
    }
}
