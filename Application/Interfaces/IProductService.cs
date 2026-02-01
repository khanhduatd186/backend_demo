using backend.Application.DTOs.Common;
using backend.Application.DTOs.Product.Requests;
using backend.Application.DTOs.Product.Responses;

namespace backend.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<PagedResponse<ProductResponse>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<ProductResponse>> GetFilteredAsync(ProductFilterRequest request);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);
    Task DeleteAsync(Guid id);
}
