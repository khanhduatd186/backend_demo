namespace backend.Application.DTOs.Language.Responses;

/// <summary>
/// DTO response cho Translation
/// </summary>
public class TranslationResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
