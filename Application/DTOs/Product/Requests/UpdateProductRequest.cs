using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Product.Requests;

public class UpdateProductRequest
{
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Image { get; set; }

    public Guid? CategoryId { get; set; }
}
