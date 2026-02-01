using Microsoft.EntityFrameworkCore;
using backend.Domain.Category.Entities;
using backend.Domain.Category.Interfaces;
using backend.Domain.Common;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;
using CategoryEntity = backend.Domain.Category.Entities.Category;

namespace backend.Infrastructure.Repositories.Category;

/// <summary>
/// Repository implementation cho Category entity
/// </summary>
public class CategoryRepository : Repository<CategoryEntity>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CategoryEntity?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbSet
            .AnyAsync(c => c.Code == code && !c.IsDeleted);
    }
}
