using backend.Domain.Common;

namespace backend.Domain.Permission.Entities;

/// <summary>
/// Permission entity - quyền truy cập trong hệ thống
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Product.Create", "Product.Read"
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., "Product", "User"
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"
    
    // Navigation property
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
