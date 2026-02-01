# API Generation Rules - Clean Architecture

## 📋 Mục lục
1. [Cấu trúc Clean Architecture](#cấu-trúc-clean-architecture)
2. [Quy trình tạo Entity mới](#quy-trình-tạo-entity-mới)
3. [Template Code](#template-code)
4. [Checklist](#checklist)
5. [Ví dụ: Tạo Entity "Order"](#ví-dụ-tạo-entity-order)

---

## 🏗️ Cấu trúc Clean Architecture

```
backend/
├── Domain/                          # Domain Layer (Entities, Interfaces)
│   └── {EntityName}/
│       ├── Entities/
│       │   └── {EntityName}.cs
│       └── Interfaces/
│           └── I{EntityName}Repository.cs
│
├── Application/                     # Application Layer (DTOs, Services, Interfaces)
│   ├── DTOs/
│   │   └── {EntityName}/
│   │       ├── Requests/
│   │       │   ├── Create{EntityName}Request.cs
│   │       │   ├── Update{EntityName}Request.cs
│   │       │   └── {EntityName}FilterRequest.cs
│   │       └── Responses/
│   │           └── {EntityName}Response.cs
│   ├── Interfaces/
│   │   └── I{EntityName}Service.cs
│   └── Services/
│       └── {EntityName}Service.cs
│
├── Infrastructure/                  # Infrastructure Layer (Repositories, DbContext)
│   ├── Repositories/
│   │   └── {EntityName}/
│   │       └── {EntityName}Repository.cs
│   └── Data/
│       └── ApplicationDbContext.cs
│
└── Controllers/                     # Presentation Layer
    └── {EntityName}Controller.cs
```

---

## 📝 Quy trình tạo Entity mới

### Bước 1: Tạo Domain Entity
**File:** `Domain/{EntityName}/Entities/{EntityName}.cs`

```csharp
using backend.Domain.Common;

namespace backend.Domain.{EntityName}.Entities;

/// <summary>
/// {EntityName} entity
/// </summary>
public class {EntityName} : BaseEntity
{
    // Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Foreign keys (nếu có)
    public Guid? RelatedEntityId { get; set; }
    
    // Navigation properties (nếu có)
    public RelatedEntity? RelatedEntity { get; set; }
}
```

**Lưu ý:**
- Kế thừa từ `BaseEntity` (có sẵn `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
- Sử dụng `string.Empty` cho string properties bắt buộc
- Sử dụng `?` cho nullable properties

---

### Bước 2: Tạo Repository Interface
**File:** `Domain/{EntityName}/Interfaces/I{EntityName}Repository.cs`

```csharp
using backend.Domain.Common;
using {EntityName}Entity = backend.Domain.{EntityName}.Entities.{EntityName};

namespace backend.Domain.{EntityName}.Interfaces;

/// <summary>
/// Repository interface cho {EntityName} entity
/// </summary>
public interface I{EntityName}Repository : IRepository<{EntityName}Entity>
{
    // Custom methods (nếu cần)
    Task<{EntityName}Entity?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
```

---

### Bước 3: Tạo DTOs

#### 3.1. Create Request
**File:** `Application/DTOs/{EntityName}/Requests/Create{EntityName}Request.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.{EntityName}.Requests;

public class Create{EntityName}Request
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Foreign keys (nếu có)
    public Guid? RelatedEntityId { get; set; }
}
```

**Lưu ý:**
- Không thêm `ErrorMessage` vào Data Annotations (sẽ được localize tự động)
- Sử dụng `[Required]` cho properties bắt buộc
- Sử dụng `[MaxLength]` cho string properties

#### 3.2. Update Request
**File:** `Application/DTOs/{EntityName}/Requests/Update{EntityName}Request.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.{EntityName}.Requests;

public class Update{EntityName}Request
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Foreign keys (nếu có)
    public Guid? RelatedEntityId { get; set; }
}
```

#### 3.3. Filter Request
**File:** `Application/DTOs/{EntityName}/Requests/{EntityName}FilterRequest.cs`

```csharp
using backend.Application.DTOs.Common;

namespace backend.Application.DTOs.{EntityName}.Requests;

/// <summary>
/// Filter request cho {EntityName} với các filter cụ thể
/// </summary>
public class {EntityName}FilterRequest : FilterRequest
{
    /// <summary>
    /// Lọc theo tên
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Lọc theo mô tả
    /// </summary>
    public string? Description { get; set; }

    // Foreign key filters (nếu có)
    /// <summary>
    /// Lọc theo mã related entity
    /// </summary>
    public string? RelatedEntityCode { get; set; }
}
```

#### 3.4. Response
**File:** `Application/DTOs/{EntityName}/Responses/{EntityName}Response.cs`

```csharp
namespace backend.Application.DTOs.{EntityName}.Responses;

public class {EntityName}Response
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? RelatedEntityId { get; set; }
    
    // Navigation properties (nếu cần)
    public RelatedEntityResponse? RelatedEntity { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### Bước 4: Tạo Service Interface
**File:** `Application/Interfaces/I{EntityName}Service.cs`

```csharp
using backend.Application.DTOs.Common;
using backend.Application.DTOs.{EntityName}.Requests;
using backend.Application.DTOs.{EntityName}.Responses;

namespace backend.Application.Interfaces;

/// <summary>
/// Service interface cho {EntityName}
/// </summary>
public interface I{EntityName}Service
{
    Task<{EntityName}Response> CreateAsync(Create{EntityName}Request request);
    Task<{EntityName}Response?> GetByIdAsync(Guid id);
    Task<PagedResponse<{EntityName}Response>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<{EntityName}Response>> GetFilteredAsync({EntityName}FilterRequest request);
    Task<{EntityName}Response> UpdateAsync(Guid id, Update{EntityName}Request request);
    Task DeleteAsync(Guid id);
}
```

---

### Bước 5: Implement Service
**File:** `Application/Services/{EntityName}Service.cs`

```csharp
using AutoMapper;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.{EntityName}.Requests;
using backend.Application.DTOs.{EntityName}.Responses;
using backend.Application.Interfaces;
using backend.Domain.{EntityName}.Entities;
using backend.Domain.{EntityName}.Interfaces;
using backend.Domain.Common;
using backend.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

/// <summary>
/// Service để quản lý {EntityName}
/// </summary>
public class {EntityName}Service : I{EntityName}Service
{
    private readonly I{EntityName}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    public {EntityName}Service(
        I{EntityName}Repository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILocalizationService localizationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    public async Task<{EntityName}Response> CreateAsync(Create{EntityName}Request request)
    {
        // Validation (nếu cần)
        if (await _repository.ExistsByNameAsync(request.Name))
        {
            throw new InvalidOperationException(_localizationService.GetString("{EntityName}NameExists"));
        }

        // Validate foreign key (nếu có)
        // if (request.RelatedEntityId.HasValue)
        // {
        //     var relatedEntity = await _relatedEntityRepository.GetByIdAsync(request.RelatedEntityId.Value);
        //     if (relatedEntity == null)
        //     {
        //         throw new KeyNotFoundException(_localizationService.GetString("RelatedEntityNotFound"));
        //     }
        // }

        var entity = _mapper.Map<{EntityName}Entity>(request);
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<{EntityName}Response>(entity);
    }

    public async Task<{EntityName}Response?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<{EntityName}Response>(entity);
    }

    public async Task<PagedResponse<{EntityName}Response>> GetPagedAsync(PagedRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allEntities = await _repository.GetAllAsync();
        var entities = allEntities.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            entities = entities.Where(e => 
                e.Name.ToLower().Contains(searchTerm) ||
                (e.Description != null && e.Description.ToLower().Contains(searchTerm)));
        }

        var totalRecords = entities.Count();
        var pagedEntities = entities
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<{EntityName}Response>
        {
            Data = _mapper.Map<IEnumerable<{EntityName}Response>>(pagedEntities),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<PagedResponse<{EntityName}Response>> GetFilteredAsync({EntityName}FilterRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allEntities = await _repository.GetAllAsync();
        var entities = allEntities.AsQueryable();

        // Apply dynamic filters
        entities = DynamicFilterHelper.ApplyDynamicFilters(entities, request);

        // Foreign key filters (nếu có)
        // if (!string.IsNullOrWhiteSpace(request.RelatedEntityCode))
        // {
        //     var relatedEntity = await _relatedEntityRepository.GetByCodeAsync(request.RelatedEntityCode);
        //     if (relatedEntity != null)
        //     {
        //         entities = entities.Where(e => e.RelatedEntityId == relatedEntity.Id);
        //     }
        //     else
        //     {
        //         entities = entities.Where(e => false);
        //     }
        // }

        // Include navigation properties (nếu có)
        // entities = entities.Include(e => e.RelatedEntity);

        var totalRecords = entities.Count();

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "createdat";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";

        entities = sortBy switch
        {
            "name" => sortDirection == "asc" 
                ? entities.OrderBy(e => e.Name) 
                : entities.OrderByDescending(e => e.Name),
            "createdat" => sortDirection == "asc" 
                ? entities.OrderBy(e => e.CreatedAt) 
                : entities.OrderByDescending(e => e.CreatedAt),
            "updatedat" => sortDirection == "asc" 
                ? entities.OrderBy(e => e.UpdatedAt ?? e.CreatedAt) 
                : entities.OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt),
            _ => entities.OrderByDescending(e => e.CreatedAt)
        };

        // Apply pagination
        var pagedEntities = entities
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<{EntityName}Response>
        {
            Data = _mapper.Map<IEnumerable<{EntityName}Response>>(pagedEntities),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<{EntityName}Response> UpdateAsync(Guid id, Update{EntityName}Request request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("{EntityName}NotFound"));
        }

        // Validation (nếu cần)
        // if (entity.Name != request.Name && await _repository.ExistsByNameAsync(request.Name))
        // {
        //     throw new InvalidOperationException(_localizationService.GetString("{EntityName}NameExists"));
        // }

        _mapper.Map(request, entity);
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<{EntityName}Response>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("{EntityName}NotFound"));
        }

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### Bước 6: Implement Repository
**File:** `Infrastructure/Repositories/{EntityName}/{EntityName}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using backend.Domain.{EntityName}.Entities;
using backend.Domain.{EntityName}.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.{EntityName};

/// <summary>
/// Repository implementation cho {EntityName} entity
/// </summary>
public class {EntityName}Repository : Repository<Domain.{EntityName}.Entities.{EntityName}>, I{EntityName}Repository
{
    public {EntityName}Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.{EntityName}.Entities.{EntityName}?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(x => x.Name == name && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet
            .AnyAsync(x => x.Name == name && !x.IsDeleted);
    }
}
```

---

### Bước 7: Cấu hình DbContext
**File:** `Infrastructure/Data/ApplicationDbContext.cs`

#### 7.1. Thêm DbSet
```csharp
public DbSet<{EntityName}Entity> {EntityName}s => Set<{EntityName}Entity>();
```

#### 7.2. Cấu hình Entity trong OnModelCreating
```csharp
// Configure {EntityName}
builder.Entity<{EntityName}Entity>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Description).HasMaxLength(500);
    entity.HasIndex(e => e.Name).IsUnique(); // Nếu cần unique

    // Foreign key relationship (nếu có)
    entity.HasOne(e => e.RelatedEntity)
        .WithMany() // hoặc .WithOne() tùy quan hệ
        .HasForeignKey(e => e.RelatedEntityId)
        .IsRequired(false) // Nếu nullable
        .OnDelete(DeleteBehavior.SetNull); // hoặc Cascade, Restrict
});
```

---

### Bước 8: Cấu hình AutoMapper
**File:** `Application/Mappings/MappingProfile.cs`

```csharp
// {EntityName} mappings
CreateMap<Domain.{EntityName}.Entities.{EntityName}, {EntityName}Response>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
    .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.RelatedEntityId))
    .ForMember(dest => dest.RelatedEntity, opt => opt.MapFrom(src => src.RelatedEntity)) // Nếu có
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

CreateMap<Create{EntityName}Request, Domain.{EntityName}.Entities.{EntityName}>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.RelatedEntity, opt => opt.Ignore()); // Ignore navigation property

CreateMap<Update{EntityName}Request, Domain.{EntityName}.Entities.{EntityName}>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.RelatedEntity, opt => opt.Ignore()); // Ignore navigation property
```

---

### Bước 9: Đăng ký Dependency Injection
**File:** `Infrastructure/DependencyInjection.cs`

```csharp
// Repositories
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();

// Services
services.AddScoped<Application.Interfaces.I{EntityName}Service, Application.Services.{EntityName}Service>();
```

---

### Bước 10: Tạo Controller
**File:** `Controllers/{EntityName}Controller.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.{EntityName}.Requests;
using backend.Application.Interfaces;
using backend.Attributes;

namespace backend.Controllers;

/// <summary>
/// Controller để quản lý {EntityName}
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {EntityName}Controller : ControllerBase
{
    private readonly I{EntityName}Service _{entityName}Service;
    private readonly ILocalizationService _localizationService;

    public {EntityName}Controller(
        I{EntityName}Service {entityName}Service, 
        ILocalizationService localizationService)
    {
        _{entityName}Service = {entityName}Service;
        _localizationService = localizationService;
    }

    [HttpPost]
    [RequirePermission("{EntityName}.Create")]
    public async Task<IActionResult> Create([FromBody] Create{EntityName}Request request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _{entityName}Service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorCreating{EntityName}"), error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("{EntityName}.Read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var response = await _{entityName}Service.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { message = _localizationService.GetString("{EntityName}NotFound") });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet]
    [RequirePermission("{EntityName}.Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var response = await _{entityName}Service.GetPagedAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("filtered")]
    [RequirePermission("{EntityName}.Read")]
    public async Task<IActionResult> GetFiltered([FromQuery] {EntityName}FilterRequest request)
    {
        try
        {
            var response = await _{entityName}Service.GetFilteredAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("{EntityName}.Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update{EntityName}Request request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _{entityName}Service.UpdateAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorUpdating{EntityName}"), error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("{EntityName}.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _{entityName}Service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorDeleting{EntityName}"), error = ex.Message });
        }
    }
}
```

---

### Bước 11: Thêm Permissions vào DatabaseSeeder
**File:** `Infrastructure/Data/DatabaseSeeder.cs`

Trong method `SeedPermissionsAsync()`, thêm:
```csharp
("{EntityName}.Create", "Create {entityName}", "{EntityName}", "Create"),
("{EntityName}.Read", "Read {entityName}", "{EntityName}", "Read"),
("{EntityName}.Update", "Update {entityName}", "{EntityName}", "Update"),
("{EntityName}.Delete", "Delete {entityName}", "{EntityName}", "Delete"),
```

---

### Bước 12: Thêm Translation Keys
**Files:** `Resources/translations.vi.json` và `Resources/translations.en.json`

```json
{
  "{EntityName}Created": "{EntityName} đã được tạo thành công",
  "{EntityName}Updated": "{EntityName} đã được cập nhật",
  "{EntityName}Deleted": "{EntityName} đã được xóa",
  "{EntityName}NotFound": "Không tìm thấy {entityName}",
  "{EntityName}NameExists": "Tên {entityName} đã tồn tại",
  "ErrorCreating{EntityName}": "Lỗi khi tạo {entityName}",
  "ErrorUpdating{EntityName}": "Lỗi khi cập nhật {entityName}",
  "ErrorDeleting{EntityName}": "Lỗi khi xóa {entityName}"
}
```

---

### Bước 13: Tạo Migration
```bash
dotnet ef migrations add Add{EntityName}Entity
dotnet ef database update
```

---

## ✅ Checklist

Khi tạo entity mới, đảm bảo đã hoàn thành:

- [ ] **Domain Layer**
  - [ ] Entity class kế thừa `BaseEntity`
  - [ ] Repository interface với custom methods (nếu cần)

- [ ] **Application Layer**
  - [ ] CreateRequest DTO với Data Annotations
  - [ ] UpdateRequest DTO
  - [ ] FilterRequest DTO (kế thừa `FilterRequest`)
  - [ ] Response DTO
  - [ ] Service interface với đầy đủ methods
  - [ ] Service implementation với validation và error handling

- [ ] **Infrastructure Layer**
  - [ ] Repository implementation
  - [ ] DbSet trong `ApplicationDbContext`
  - [ ] Entity configuration trong `OnModelCreating`

- [ ] **Presentation Layer**
  - [ ] Controller với đầy đủ endpoints
  - [ ] `[RequirePermission]` attributes cho từng endpoint

- [ ] **Configuration**
  - [ ] AutoMapper mappings
  - [ ] Dependency Injection registration
  - [ ] Permissions trong `DatabaseSeeder`
  - [ ] Translation keys trong JSON files

- [ ] **Database**
  - [ ] Migration created và applied

---

## 📚 Ví dụ: Tạo Entity "Order"

### 1. Entity
```csharp
// Domain/Order/Entities/Order.cs
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
```

### 2. Repository Interface
```csharp
// Domain/Order/Interfaces/IOrderRepository.cs
public interface IOrderRepository : IRepository<OrderEntity>
{
    Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber);
    Task<bool> ExistsByOrderNumberAsync(string orderNumber);
}
```

### 3. DTOs
```csharp
// Application/DTOs/Order/Requests/CreateOrderRequest.cs
public class CreateOrderRequest
{
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    public Guid? CustomerId { get; set; }
}
```

### 4. Service
```csharp
// Application/Services/OrderService.cs
public class OrderService : IOrderService
{
    // Implement các methods theo template
}
```

### 5. Controller
```csharp
// Controllers/OrderController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    // Implement các endpoints theo template
}
```

---

## 🔑 Lưu ý quan trọng

1. **Naming Convention:**
   - Entity: `{EntityName}` (PascalCase, singular)
   - Repository Interface: `I{EntityName}Repository`
   - Service Interface: `I{EntityName}Service`
   - Controller: `{EntityName}Controller`
   - DTOs: `Create{EntityName}Request`, `Update{EntityName}Request`, `{EntityName}Response`

2. **Permissions:**
   - Format: `{EntityName}.{Action}` (ví dụ: `Order.Create`, `Order.Read`)
   - Phải thêm vào `DatabaseSeeder` để seed vào database

3. **Validation:**
   - Sử dụng Data Annotations trong DTOs
   - Không thêm `ErrorMessage` (sẽ được localize tự động)
   - Thêm validation logic trong Service nếu cần

4. **Error Handling:**
   - Sử dụng `ILocalizationService` cho tất cả error messages
   - Throw `KeyNotFoundException` khi không tìm thấy
   - Throw `InvalidOperationException` khi business rule vi phạm

5. **Foreign Keys:**
   - Nếu có foreign key, validate trong Service
   - Thêm filter trong `FilterRequest` nếu cần filter theo foreign key
   - Include navigation properties trong queries nếu cần

6. **Soft Delete:**
   - Tất cả entities đều có `IsDeleted` từ `BaseEntity`
   - Repository tự động filter `IsDeleted = false`
   - Sử dụng `DeleteAsync()` để soft delete

---

## 🚀 Quick Start

1. Copy template code và thay thế `{EntityName}` bằng tên entity của bạn
2. Thay thế `{entityName}` bằng tên entity ở dạng lowercase
3. Điều chỉnh properties theo yêu cầu
4. Thêm validation và business logic
5. Chạy migration
6. Test API endpoints

---

**Chúc bạn code vui vẻ! 🎉**
