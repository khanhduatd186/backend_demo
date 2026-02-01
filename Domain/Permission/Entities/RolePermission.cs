using backend.Domain.Common;

namespace backend.Domain.Permission.Entities;

/// <summary>
/// RolePermission entity - liên kết Role với Permission
/// </summary>
public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    
    // Navigation properties
    public Permission Permission { get; set; } = null!;
}
