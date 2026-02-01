using backend.Domain.Common;

namespace backend.Domain.Category.Entities;

/// <summary>
/// Category entity
/// </summary>
public class Category : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Domain.Product.Entities.Product> Products { get; set; } = new List<Domain.Product.Entities.Product>();
}
