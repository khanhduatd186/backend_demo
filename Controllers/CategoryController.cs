using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Category.Requests;
using backend.Application.Interfaces;
using backend.Attributes;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILocalizationService _localizationService;

    public CategoryController(ICategoryService categoryService, ILocalizationService localizationService)
    {
        _categoryService = categoryService;
        _localizationService = localizationService;
    }

    [HttpPost]
    [RequirePermission("Category.Create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _categoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorCreatingCategory"), error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("Category.Read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var response = await _categoryService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { message = _localizationService.GetString("CategoryNotFound") });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet]
    [RequirePermission("Category.Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var response = await _categoryService.GetPagedAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("filtered")]
    [RequirePermission("Category.Read")]
    public async Task<IActionResult> GetFiltered([FromQuery] CategoryFilterRequest request)
    {
        try
        {
            var response = await _categoryService.GetFilteredAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("Category.Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _categoryService.UpdateAsync(id, request);
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
            return StatusCode(500, new { message = _localizationService.GetString("ErrorUpdatingCategory"), error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("Category.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorDeletingCategory"), error = ex.Message });
        }
    }
}
