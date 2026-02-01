namespace backend.Application.DTOs.Language.Responses;

/// <summary>
/// DTO response cho Language
/// </summary>
public class LanguageResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
