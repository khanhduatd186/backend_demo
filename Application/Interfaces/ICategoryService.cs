using backend.Application.DTOs.Common;
using backend.Application.DTOs.Category.Requests;
using backend.Application.DTOs.Category.Responses;

namespace backend.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<PagedResponse<CategoryResponse>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<CategoryResponse>> GetFilteredAsync(CategoryFilterRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteAsync(Guid id);
}
