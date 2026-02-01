using backend.Application.DTOs.Common;

namespace backend.Application.DTOs.Category.Requests;

/// <summary>
/// Filter request cho Category với các filter cụ thể
/// </summary>
public class CategoryFilterRequest : FilterRequest
{
    /// <summary>
    /// Lọc theo mã category
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Lọc theo description
    /// </summary>
    public string? Description { get; set; }
}
