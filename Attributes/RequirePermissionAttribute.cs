using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace backend.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permissionName;

    public RequirePermissionAttribute(string permissionName)
    {
        _permissionName = permissionName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if user has the required permission
        var permissions = user.FindAll("Permission").Select(c => c.Value).ToList();
        
        // Log để debug
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequirePermissionAttribute>>();
        logger.LogInformation("Checking permission: {PermissionName}. User has permissions: {Permissions}", 
            _permissionName, string.Join(", ", permissions));
        
        if (!permissions.Contains(_permissionName))
        {
            logger.LogWarning("Permission denied. Required: {PermissionName}, User has: {Permissions}", 
                _permissionName, string.Join(", ", permissions));
            context.Result = new ForbidResult();
        }
    }
}
