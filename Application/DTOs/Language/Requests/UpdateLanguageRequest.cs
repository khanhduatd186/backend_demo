using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Language.Requests;

/// <summary>
/// DTO để cập nhật ngôn ngữ của user
/// </summary>
public class UpdateLanguageRequest
{
    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string Language { get; set; } = "vi";
}
