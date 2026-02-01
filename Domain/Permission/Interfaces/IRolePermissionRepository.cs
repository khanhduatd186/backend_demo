using backend.Domain.Common;
using RolePermissionEntity = backend.Domain.Permission.Entities.RolePermission;

namespace backend.Domain.Permission.Interfaces;

/// <summary>
/// Repository interface cho RolePermission entity
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermissionEntity>
{
    Task<IEnumerable<RolePermissionEntity>> GetByRoleIdAsync(string roleId);
    Task<RolePermissionEntity?> GetByRoleIdAndPermissionIdAsync(string roleId, Guid permissionId);
    Task<RolePermissionEntity?> GetByRoleIdAndPermissionIdIncludingDeletedAsync(string roleId, Guid permissionId);
    Task DeleteByRoleIdAsync(string roleId);
    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName);
}
