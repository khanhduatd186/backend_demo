using backend.Domain.Common;
using ProductEntity = backend.Domain.Product.Entities.Product;

namespace backend.Domain.Product.Interfaces;

/// <summary>
/// Repository interface cho Product entity
/// </summary>
public interface IProductRepository : IRepository<ProductEntity>
{
    Task<ProductEntity?> GetByProductCodeAsync(string productCode);
    Task<bool> ExistsByProductCodeAsync(string productCode);
}
