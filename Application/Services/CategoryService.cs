using AutoMapper;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Category.Requests;
using backend.Application.DTOs.Category.Responses;
using backend.Application.Interfaces;
using backend.Application.Helpers;
using CategoryEntity = backend.Domain.Category.Entities.Category;
using backend.Domain.Category.Interfaces;
using backend.Domain.Common;

namespace backend.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    public CategoryService(
        ICategoryRepository repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ILocalizationService localizationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        // Check if category code already exists
        if (await _repository.ExistsByCodeAsync(request.Code))
        {
            throw new InvalidOperationException(_localizationService.GetString("CategoryCodeExists", request.Code));
        }

        var category = _mapper.Map<CategoryEntity>(request);
        await _repository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category == null ? null : _mapper.Map<CategoryResponse>(category);
    }

    public async Task<PagedResponse<CategoryResponse>> GetPagedAsync(PagedRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allCategories = await _repository.GetAllAsync();
        var categories = allCategories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            categories = categories.Where(c => 
                c.Code.ToLower().Contains(searchTerm) ||
                c.Description.ToLower().Contains(searchTerm));
        }

        var totalRecords = categories.Count();
        var pagedCategories = categories
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<CategoryResponse>
        {
            Data = _mapper.Map<IEnumerable<CategoryResponse>>(pagedCategories),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<PagedResponse<CategoryResponse>> GetFilteredAsync(CategoryFilterRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allCategories = await _repository.GetAllAsync();
        var categories = allCategories.AsQueryable();

        // Apply dynamic filters using helper
        categories = DynamicFilterHelper.ApplyDynamicFilters(categories, request);

        // Get total count
        var totalRecords = categories.Count();

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "createdat";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";

        categories = sortBy switch
        {
            "code" => sortDirection == "asc" 
                ? categories.OrderBy(c => c.Code) 
                : categories.OrderByDescending(c => c.Code),
            "description" => sortDirection == "asc" 
                ? categories.OrderBy(c => c.Description) 
                : categories.OrderByDescending(c => c.Description),
            "createdat" => sortDirection == "asc" 
                ? categories.OrderBy(c => c.CreatedAt) 
                : categories.OrderByDescending(c => c.CreatedAt),
            "updatedat" => sortDirection == "asc" 
                ? categories.OrderBy(c => c.UpdatedAt ?? c.CreatedAt) 
                : categories.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt),
            _ => categories.OrderByDescending(c => c.CreatedAt)
        };

        // Apply pagination
        var pagedCategories = categories
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<CategoryResponse>
        {
            Data = _mapper.Map<IEnumerable<CategoryResponse>>(pagedCategories),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("CategoryNotFound"));
        }

        // Check if code is being changed and if new code already exists
        if (category.Code != request.Code)
        {
            if (await _repository.ExistsByCodeAsync(request.Code))
            {
                throw new InvalidOperationException(_localizationService.GetString("CategoryCodeExists", request.Code));
            }
        }

        _mapper.Map(request, category);
        await _repository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("CategoryNotFound"));
        }

        await _repository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
