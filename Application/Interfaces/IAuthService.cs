using backend.Application.DTOs.Auth.Requests;
using backend.Application.DTOs.Auth.Responses;
using backend.Application.DTOs.User.Responses;

namespace backend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserResponse> GetCurrentUserAsync(string userId);
    Task<string> UpdateLanguageAsync(string userId, string language);
}
