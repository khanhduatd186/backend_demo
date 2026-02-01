using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Auth.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
