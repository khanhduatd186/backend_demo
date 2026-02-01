using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs.Permission.Requests;

public class AssignPermissionsToRoleRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    public List<Guid> PermissionIds { get; set; } = new();
}
