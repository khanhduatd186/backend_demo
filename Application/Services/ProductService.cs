using AutoMapper;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Product.Requests;
using backend.Application.DTOs.Product.Responses;
using backend.Application.Interfaces;
using backend.Application.Helpers;
using backend.Domain.Product.Entities;
using backend.Domain.Product.Interfaces;
using backend.Domain.Category.Interfaces;
using backend.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    public ProductService(
        IProductRepository repository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ILocalizationService localizationService)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        // Check if product code already exists
        if (await _repository.ExistsByProductCodeAsync(request.ProductCode))
        {
            throw new InvalidOperationException(_localizationService.GetString("ProductCodeExists"));
        }

        // Check if CategoryId exists (if provided)
        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
            {
                throw new KeyNotFoundException(_localizationService.GetString("CategoryNotFound"));
            }
        }

        var product = _mapper.Map<Domain.Product.Entities.Product>(request);
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Load Category for response
        var allProducts = await _repository.GetAllAsync();
        var productWithCategory = allProducts.AsQueryable()
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == product.Id);

        return _mapper.Map<ProductResponse>(productWithCategory ?? product);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<ProductResponse>(product);
    }

    public async Task<PagedResponse<ProductResponse>> GetPagedAsync(PagedRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        // Get all products and filter
        var allProducts = await _repository.GetAllAsync();
        var products = allProducts.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            products = products.Where(p => 
                p.ProductCode.ToLower().Contains(searchTerm) ||
                p.ProductName.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalRecords = products.Count();

        // Apply pagination
        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<ProductResponse>
        {
            Data = _mapper.Map<IEnumerable<ProductResponse>>(pagedProducts),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<PagedResponse<ProductResponse>> GetFilteredAsync(ProductFilterRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        // Get all products
        var allProducts = await _repository.GetAllAsync();
        var products = allProducts.AsQueryable();

        // Áp dụng dynamic filters (tự động filter theo tất cả properties trong request)
        products = DynamicFilterHelper.ApplyDynamicFilters(products, request);

        // Filter theo HasImage (special case)
        if (request.HasImage.HasValue)
        {
            if (request.HasImage.Value)
            {
                products = products.Where(p => !string.IsNullOrEmpty(p.Image));
            }
            else
            {
                products = products.Where(p => string.IsNullOrEmpty(p.Image));
            }
        }

        // Filter theo CategoryCode (foreign key filter)
        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            var category = await _categoryRepository.GetByCodeAsync(request.CategoryCode);
            if (category != null)
            {
                products = products.Where(p => p.CategoryId == category.Id);
            }
            else
            {
                // Nếu không tìm thấy category, trả về empty result
                products = products.Where(p => false);
            }
        }

        // Include Category để load navigation property
        products = products.Include(p => p.Category);

        // Get total count trước khi sort và paginate
        var totalRecords = products.Count();

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "createdat";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";

        products = sortBy switch
        {
            "productcode" => sortDirection == "asc" 
                ? products.OrderBy(p => p.ProductCode) 
                : products.OrderByDescending(p => p.ProductCode),
            "productname" => sortDirection == "asc" 
                ? products.OrderBy(p => p.ProductName) 
                : products.OrderByDescending(p => p.ProductName),
            "createdat" => sortDirection == "asc" 
                ? products.OrderBy(p => p.CreatedAt) 
                : products.OrderByDescending(p => p.CreatedAt),
            "updatedat" => sortDirection == "asc" 
                ? products.OrderBy(p => p.UpdatedAt ?? p.CreatedAt) 
                : products.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt),
            _ => products.OrderByDescending(p => p.CreatedAt)
        };

        // Apply pagination
        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<ProductResponse>
        {
            Data = _mapper.Map<IEnumerable<ProductResponse>>(pagedProducts),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("ProductNotFound"));
        }

        // Check if product code is being changed and if new code already exists
        if (product.ProductCode != request.ProductCode)
        {
            if (await _repository.ExistsByProductCodeAsync(request.ProductCode))
            {
                throw new InvalidOperationException(_localizationService.GetString("ProductCodeExists"));
            }
        }

        // Check if CategoryId exists (if provided and changed)
        if (request.CategoryId.HasValue && product.CategoryId != request.CategoryId.Value)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
            {
                throw new KeyNotFoundException(_localizationService.GetString("CategoryNotFound"));
            }
        }

        _mapper.Map(request, product);
        await _repository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Load Category for response
        var allProducts = await _repository.GetAllAsync();
        var productWithCategory = allProducts.AsQueryable()
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == product.Id);

        return _mapper.Map<ProductResponse>(productWithCategory ?? product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("ProductNotFound"));
        }

        await _repository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
