using Microsoft.EntityFrameworkCore;
using backend.Domain.Permission.Entities;
using backend.Domain.Permission.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Permission;

/// <summary>
/// Repository implementation cho RolePermission entity
/// </summary>
public class RolePermissionRepository : Repository<Domain.Permission.Entities.RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Domain.Permission.Entities.RolePermission>> GetByRoleIdAsync(string roleId)
    {
        return await _dbSet
            .Where(x => x.RoleId == roleId && !x.IsDeleted)
            .Include(x => x.Permission)
            .ToListAsync();
    }

    public async Task<Domain.Permission.Entities.RolePermission?> GetByRoleIdAndPermissionIdAsync(string roleId, Guid permissionId)
    {
        return await _dbSet
            .Where(x => x.RoleId == roleId && x.PermissionId == permissionId && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get RolePermission by RoleId and PermissionId, including soft deleted ones
    /// </summary>
    public async Task<Domain.Permission.Entities.RolePermission?> GetByRoleIdAndPermissionIdIncludingDeletedAsync(string roleId, Guid permissionId)
    {
        return await _dbSet
            .Where(x => x.RoleId == roleId && x.PermissionId == permissionId)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteByRoleIdAsync(string roleId)
    {
        var rolePermissions = await GetByRoleIdAsync(roleId);
        foreach (var rolePermission in rolePermissions)
        {
            await DeleteAsync(rolePermission);
        }
    }

    public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionName)
    {
        return await _dbSet
            .Where(x => x.RoleId == roleId && !x.IsDeleted)
            .Where(x => x.Permission.Name == permissionName && !x.Permission.IsDeleted)
            .AnyAsync();
    }
}
