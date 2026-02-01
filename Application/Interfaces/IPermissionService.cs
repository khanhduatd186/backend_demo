using backend.Application.DTOs.Common;
using backend.Application.DTOs.Permission.Requests;
using backend.Application.DTOs.Permission.Responses;

namespace backend.Application.Interfaces;

public interface IPermissionService
{
    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request);
    Task<PermissionResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<PermissionResponse>> GetAllAsync();
    Task<PagedResponse<PermissionResponse>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<PermissionResponse>> GetFilteredAsync(PermissionFilterRequest request);
    Task<PermissionResponse> UpdateAsync(Guid id, UpdatePermissionRequest request);
    Task DeleteAsync(Guid id);
    Task AssignPermissionsToRoleAsync(AssignPermissionsToRoleRequest request);
    Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleAsync(string roleName);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
}
