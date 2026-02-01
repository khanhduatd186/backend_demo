using backend.Domain.Common;

namespace backend.Domain.Language.Entities;

/// <summary>
/// Entity cho ngôn ngữ (vi, en, ...)
/// </summary>
public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty; // vi, en, ...
    public string Name { get; set; } = string.Empty; // Tiếng Việt, English, ...
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    // Navigation property
    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
}
