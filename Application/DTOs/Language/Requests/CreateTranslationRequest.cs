using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Language.Requests;

/// <summary>
/// DTO để tạo mới translation
/// </summary>
public class CreateTranslationRequest
{
    [Required]
    [MaxLength(255)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string LanguageCode { get; set; } = string.Empty;
}
