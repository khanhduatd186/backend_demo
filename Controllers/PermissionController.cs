using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Permission.Requests;
using backend.Application.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    public PermissionController(IPermissionService permissionService, ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _permissionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorCreatingPermission"), error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var response = await _permissionService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { message = _localizationService.GetString("PermissionNotFound") });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var response = await _permissionService.GetAllAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var response = await _permissionService.GetPagedAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFiltered([FromQuery] PermissionFilterRequest request)
    {
        try
        {
            var response = await _permissionService.GetFilteredAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _permissionService.UpdateAsync(id, request);
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
            return StatusCode(500, new { message = _localizationService.GetString("ErrorUpdatingPermission"), error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _permissionService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorDeletingPermission"), error = ex.Message });
        }
    }

    [HttpPost("assign-to-role")]
    public async Task<IActionResult> AssignPermissionsToRole([FromBody] AssignPermissionsToRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _permissionService.AssignPermissionsToRoleAsync(request);
            return Ok(new { message = _localizationService.GetString("PermissionsAssignedSuccess") });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorAssigningPermissions"), error = ex.Message });
        }
    }

    [HttpGet("by-role/{roleName}")]
    public async Task<IActionResult> GetPermissionsByRole(string roleName)
    {
        try
        {
            var response = await _permissionService.GetPermissionsByRoleAsync(roleName);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }
}
