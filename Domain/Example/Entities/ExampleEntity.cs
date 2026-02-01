using backend.Domain.Common;

namespace backend.Domain.Example.Entities;

/// <summary>
/// Example entity - entity mẫu để tham khảo
/// </summary>
public class ExampleEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
