using backend.Domain.Common;

namespace backend.Domain.Product.Entities;

/// <summary>
/// Product entity
/// </summary>
public class Product : BaseEntity
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Image { get; set; }
    public Guid? CategoryId { get; set; }
    
    // Navigation property
    public Domain.Category.Entities.Category? Category { get; set; }
}
