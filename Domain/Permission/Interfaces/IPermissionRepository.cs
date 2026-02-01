using backend.Domain.Common;
using PermissionEntity = backend.Domain.Permission.Entities.Permission;

namespace backend.Domain.Permission.Interfaces;

/// <summary>
/// Repository interface cho Permission entity
/// </summary>
public interface IPermissionRepository : IRepository<PermissionEntity>
{
    Task<PermissionEntity?> GetByNameAsync(string name);
    Task<PermissionEntity?> GetByResourceAndActionAsync(string resource, string action);
    Task<IEnumerable<PermissionEntity>> GetByResourceAsync(string resource);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<PermissionEntity>> GetPermissionsByRoleIdAsync(string roleId);
    Task<IEnumerable<PermissionEntity>> GetPermissionsByUserRolesAsync(IList<string> roleIds);
}
