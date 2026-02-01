using backend.Application.DTOs.Category.Responses;

namespace backend.Application.DTOs.Product.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Image { get; set; }
    public Guid? CategoryId { get; set; }
    public CategoryResponse? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
