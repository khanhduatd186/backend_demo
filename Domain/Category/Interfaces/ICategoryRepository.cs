using backend.Domain.Common;
using CategoryEntity = backend.Domain.Category.Entities.Category;

namespace backend.Domain.Category.Interfaces;

/// <summary>
/// Repository interface cho Category entity
/// </summary>
public interface ICategoryRepository : IRepository<CategoryEntity>
{
    Task<CategoryEntity?> GetByCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code);
}
