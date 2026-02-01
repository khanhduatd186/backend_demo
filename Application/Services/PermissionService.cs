using AutoMapper;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Permission.Requests;
using backend.Application.DTOs.Permission.Responses;
using backend.Application.Interfaces;
using backend.Domain.Permission.Entities;
using backend.Domain.Permission.Interfaces;
using backend.Domain.User.Interfaces;
using backend.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace backend.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserRepository _userRepository;
    private readonly ILocalizationService _localizationService;

    public PermissionService(
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        RoleManager<IdentityRole> roleManager,
        IUserRepository userRepository,
        ILocalizationService localizationService)
    {
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _localizationService = localizationService;
    }

    public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request)
    {
        // Check if permission name already exists
        if (await _permissionRepository.ExistsByNameAsync(request.Name))
        {
            throw new InvalidOperationException(_localizationService.GetString("PermissionNameExists"));
        }

        // Check if resource+action combination already exists
        var existing = await _permissionRepository.GetByResourceAndActionAsync(request.Resource, request.Action);
        if (existing != null)
        {
            throw new InvalidOperationException(_localizationService.GetString("PermissionResourceActionExists"));
        }

        var permission = _mapper.Map<Domain.Permission.Entities.Permission>(request);
        await _permissionRepository.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PermissionResponse>(permission);
    }

    public async Task<PermissionResponse?> GetByIdAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        return permission == null ? null : _mapper.Map<PermissionResponse>(permission);
    }

    public async Task<IEnumerable<PermissionResponse>> GetAllAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PermissionResponse>>(permissions);
    }

    public async Task<PagedResponse<PermissionResponse>> GetPagedAsync(PagedRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allPermissions = await _permissionRepository.GetAllAsync();
        var permissions = allPermissions.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            permissions = permissions.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Resource.ToLower().Contains(searchTerm) ||
                p.Action.ToLower().Contains(searchTerm));
        }

        var totalRecords = permissions.Count();
        var pagedPermissions = permissions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<PermissionResponse>
        {
            Data = _mapper.Map<IEnumerable<PermissionResponse>>(pagedPermissions),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<PagedResponse<PermissionResponse>> GetFilteredAsync(PermissionFilterRequest request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

        var allPermissions = await _permissionRepository.GetAllAsync();
        var permissions = allPermissions.AsQueryable();

        // Filter theo Name
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            permissions = permissions.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
        }

        // Filter theo Resource
        if (!string.IsNullOrWhiteSpace(request.Resource))
        {
            permissions = permissions.Where(p => p.Resource.ToLower().Contains(request.Resource.ToLower()));
        }

        // Filter theo Action
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            permissions = permissions.Where(p => p.Action.ToLower().Contains(request.Action.ToLower()));
        }

        // Filter theo SearchTerm (tìm kiếm chung)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            permissions = permissions.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Resource.ToLower().Contains(searchTerm) ||
                p.Action.ToLower().Contains(searchTerm));
        }

        // Filter theo IsDeleted
        if (request.IsDeleted.HasValue)
        {
            permissions = permissions.Where(p => p.IsDeleted == request.IsDeleted.Value);
        }

        // Filter theo CreatedFrom
        if (request.CreatedFrom.HasValue)
        {
            permissions = permissions.Where(p => p.CreatedAt >= request.CreatedFrom.Value);
        }

        // Filter theo CreatedTo
        if (request.CreatedTo.HasValue)
        {
            permissions = permissions.Where(p => p.CreatedAt <= request.CreatedTo.Value);
        }

        // Get total count trước khi sort và paginate
        var totalRecords = permissions.Count();

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "createdat";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";

        permissions = sortBy switch
        {
            "name" => sortDirection == "asc" 
                ? permissions.OrderBy(p => p.Name) 
                : permissions.OrderByDescending(p => p.Name),
            "resource" => sortDirection == "asc" 
                ? permissions.OrderBy(p => p.Resource) 
                : permissions.OrderByDescending(p => p.Resource),
            "action" => sortDirection == "asc" 
                ? permissions.OrderBy(p => p.Action) 
                : permissions.OrderByDescending(p => p.Action),
            "createdat" => sortDirection == "asc" 
                ? permissions.OrderBy(p => p.CreatedAt) 
                : permissions.OrderByDescending(p => p.CreatedAt),
            "updatedat" => sortDirection == "asc" 
                ? permissions.OrderBy(p => p.UpdatedAt ?? p.CreatedAt) 
                : permissions.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt),
            _ => permissions.OrderByDescending(p => p.CreatedAt)
        };

        // Apply pagination
        var pagedPermissions = permissions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<PermissionResponse>
        {
            Data = _mapper.Map<IEnumerable<PermissionResponse>>(pagedPermissions),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }

    public async Task<PermissionResponse> UpdateAsync(Guid id, UpdatePermissionRequest request)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("PermissionNotFound"));
        }

        // Check if name is being changed and if new name already exists
        if (permission.Name != request.Name)
        {
            if (await _permissionRepository.ExistsByNameAsync(request.Name))
            {
                throw new InvalidOperationException(_localizationService.GetString("PermissionNameExists"));
            }
        }

        _mapper.Map(request, permission);
        await _permissionRepository.UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PermissionResponse>(permission);
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("PermissionNotFound"));
        }

        await _permissionRepository.DeleteAsync(permission);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AssignPermissionsToRoleAsync(AssignPermissionsToRoleRequest request)
    {
        // Check if role exists
        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("RoleNotFound"));
        }

        // Remove existing permissions for this role
        var existingRolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(role.Id);
        foreach (var rolePermission in existingRolePermissions)
        {
            await _rolePermissionRepository.DeleteAsync(rolePermission);
        }

        // Add new permissions
        foreach (var permissionId in request.PermissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                throw new KeyNotFoundException(_localizationService.GetString("PermissionNotFound"));
            }

            var rolePermission = new Domain.Permission.Entities.RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            };

            await _rolePermissionRepository.AddAsync(rolePermission);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("RoleNotFound"));
        }

        var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);
        return _mapper.Map<IEnumerable<PermissionResponse>>(permissions);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("UserNotFound"));
        }

        var roles = await _userRepository.GetRolesAsync(user);
        var roleIds = new List<string>();
        
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roleIds.Add(role.Id);
            }
        }

        var permissions = await _permissionRepository.GetPermissionsByUserRolesAsync(roleIds);
        return permissions.Select(p => p.Name).Distinct();
    }
}
