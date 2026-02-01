using Microsoft.EntityFrameworkCore;
using backend.Domain.Permission.Entities;
using backend.Domain.Permission.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Permission;

/// <summary>
/// Repository implementation cho Permission entity
/// </summary>
public class PermissionRepository : Repository<Domain.Permission.Entities.Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Permission.Entities.Permission?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(x => x.Name == name && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Permission.Entities.Permission?> GetByResourceAndActionAsync(string resource, string action)
    {
        return await _dbSet
            .Where(x => x.Resource == resource && x.Action == action && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Domain.Permission.Entities.Permission>> GetByResourceAsync(string resource)
    {
        return await _dbSet
            .Where(x => x.Resource == resource && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet
            .AnyAsync(x => x.Name == name && !x.IsDeleted);
    }

    public async Task<IEnumerable<Domain.Permission.Entities.Permission>> GetPermissionsByRoleIdAsync(string roleId)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId && !rp.IsDeleted))
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Permission.Entities.Permission>> GetPermissionsByUserRolesAsync(IList<string> roleIds)
    {
        if (!roleIds.Any())
        {
            return new List<Domain.Permission.Entities.Permission>();
        }

        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Where(p => p.RolePermissions.Any(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted))
            .ToListAsync();
    }
}
