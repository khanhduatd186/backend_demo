using Microsoft.AspNetCore.Identity;

namespace backend.Domain.User.Entities;

/// <summary>
/// User entity - kế thừa từ IdentityUser
/// </summary>
public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Ngôn ngữ của user (vi, en, ...) - mặc định là "vi"
    /// </summary>
    public string Language { get; set; } = "vi";
}
