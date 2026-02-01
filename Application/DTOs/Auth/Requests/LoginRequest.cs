using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Auth.Requests;

/// <summary>
/// DTO cho yêu cầu đăng nhập - có thể dùng username hoặc email
/// </summary>
public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
