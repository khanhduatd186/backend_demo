namespace backend.Application.DTOs.Common;

/// <summary>
/// Base class cho filter request với pagination và sorting
/// </summary>
public class FilterRequest : PagedRequest
{
    /// <summary>
    /// Field để sort (mặc định: CreatedAt)
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Hướng sort: "asc" hoặc "desc" (mặc định: "desc")
    /// </summary>
    public string? SortDirection { get; set; } = "desc";

    /// <summary>
    /// Lọc theo trạng thái IsDeleted (null = tất cả, true = đã xóa, false = chưa xóa)
    /// </summary>
    public bool? IsDeleted { get; set; }

    /// <summary>
    /// Lọc theo ngày tạo từ
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Lọc theo ngày tạo đến
    /// </summary>
    public DateTime? CreatedTo { get; set; }
}
