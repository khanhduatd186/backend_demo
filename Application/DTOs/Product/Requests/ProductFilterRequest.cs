using backend.Application.DTOs.Common;

namespace backend.Application.DTOs.Product.Requests;

/// <summary>
/// Filter request cho Product với các filter cụ thể
/// </summary>
public class ProductFilterRequest : FilterRequest
{
    /// <summary>
    /// Lọc theo mã sản phẩm
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// Lọc theo tên sản phẩm
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Lọc sản phẩm có hình ảnh (true) hoặc không có (false)
    /// </summary>
    public bool? HasImage { get; set; }

    /// <summary>
    /// Lọc theo CategoryId
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Lọc theo Category Code
    /// </summary>
    public string? CategoryCode { get; set; }
}
