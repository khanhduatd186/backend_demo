using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using backend.Application.Interfaces;
using backend.Domain.User.Entities;
using backend.Models;

namespace backend.Infrastructure.Services;

/// <summary>
/// Service xử lý JWT Token - Tạo token, refresh token và validate token
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Khởi tạo JwtService với cấu hình từ appsettings.json
    /// </summary>
    public JwtService(IConfiguration configuration)
    {
        _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() 
            ?? throw new InvalidOperationException("JwtSettings not found");
    }

    /// <summary>
    /// Tạo JWT Token với thông tin user, roles và permissions
    /// </summary>
    /// <param name="user">Thông tin user</param>
    /// <param name="roles">Danh sách roles của user</param>
    /// <param name="permissions">Danh sách permissions của user</param>
    /// <returns>JWT Token string</returns>
    public string GenerateToken(Domain.User.Entities.User user, IList<string> roles, IList<string> permissions)
    {
        // Tạo danh sách claims cơ bản
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), // User ID
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty), // Email
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty), // Username
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID - unique identifier
        };

        // Thêm tất cả roles vào claims (mỗi role là một claim riêng)
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Thêm tất cả permissions vào claims (mỗi permission là một claim riêng)
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        // Tạo signing key từ SecretKey trong config
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        // Tạo signing credentials với thuật toán HmacSha256
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tạo JWT token với các thông tin đã cấu hình
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer, // Người phát hành token
            audience: _jwtSettings.Audience, // Đối tượng sử dụng token
            claims: claims, // Danh sách claims
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes), // Thời gian hết hạn
            signingCredentials: credentials // Thông tin ký token
        );

        // Chuyển token thành string để trả về
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Tạo Refresh Token ngẫu nhiên (dùng để refresh access token khi hết hạn)
    /// </summary>
    /// <returns>Refresh token string (Base64 encoded)</returns>
    public string GenerateRefreshToken()
    {
        // Tạo mảng byte ngẫu nhiên 32 bytes
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        // Chuyển sang Base64 string để dễ lưu trữ và truyền tải
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Lấy ClaimsPrincipal từ expired token (dùng cho refresh token flow)
    /// </summary>
    /// <param name="token">Expired JWT token</param>
    /// <returns>ClaimsPrincipal chứa thông tin từ token, null nếu token không hợp lệ</returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        // Cấu hình validation cho expired token (không validate lifetime)
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Không validate audience khi refresh
            ValidateIssuer = false, // Không validate issuer khi refresh
            ValidateIssuerSigningKey = true, // Vẫn phải validate signing key
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false // Không validate lifetime vì token đã hết hạn
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        // Validate token và lấy principal
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        // Kiểm tra token có phải JWT và dùng đúng thuật toán HmacSha256 không
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
