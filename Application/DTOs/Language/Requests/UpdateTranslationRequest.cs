using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Language.Requests;

/// <summary>
/// DTO để cập nhật translation
/// </summary>
public class UpdateTranslationRequest
{
    [Required]
    public string Value { get; set; } = string.Empty;
}
