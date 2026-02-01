using backend.Domain.Common;

namespace backend.Domain.Language.Entities;

/// <summary>
/// Entity cho bản dịch theo key và ngôn ngữ
/// </summary>
public class Translation : BaseEntity
{
    public string Key { get; set; } = string.Empty; // Welcome, LoginSuccess, ...
    public string Value { get; set; } = string.Empty; // Giá trị đã dịch
    public Guid LanguageId { get; set; }
    
    // Navigation property
    public Language Language { get; set; } = null!;
}
