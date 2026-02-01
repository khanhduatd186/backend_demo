using Microsoft.EntityFrameworkCore;
using backend.Domain.Product.Entities;
using backend.Domain.Product.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Product;

/// <summary>
/// Repository implementation cho Product entity
/// </summary>
public class ProductRepository : Repository<Domain.Product.Entities.Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Product.Entities.Product?> GetByProductCodeAsync(string productCode)
    {
        return await _dbSet
            .Where(x => x.ProductCode == productCode && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByProductCodeAsync(string productCode)
    {
        return await _dbSet
            .AnyAsync(x => x.ProductCode == productCode && !x.IsDeleted);
    }
}
