using backend.Application.DTOs.Common;

namespace backend.Application.DTOs.Permission.Requests;

/// <summary>
/// Filter request cho Permission với các filter cụ thể
/// </summary>
public class PermissionFilterRequest : FilterRequest
{
    /// <summary>
    /// Lọc theo tên quyền
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Lọc theo resource
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Lọc theo action
    /// </summary>
    public string? Action { get; set; }
}
