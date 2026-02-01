using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Category.Requests;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
