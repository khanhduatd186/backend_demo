using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.DTOs.Common;
using backend.Application.DTOs.Product.Requests;
using backend.Application.Interfaces;
using backend.Attributes;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILocalizationService _localizationService;

    public ProductController(IProductService productService, ILocalizationService localizationService)
    {
        _productService = productService;
        _localizationService = localizationService;
    }

    [HttpPost]
    [RequirePermission("Product.Create")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorCreatingProduct"), error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("Product.Read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var response = await _productService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { message = _localizationService.GetString("ProductNotFound") });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet]
    [RequirePermission("Product.Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var response = await _productService.GetPagedAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpGet("filtered")]
    [RequirePermission("Product.Read")]
    public async Task<IActionResult> GetFiltered([FromQuery] ProductFilterRequest request)
    {
        try
        {
            var response = await _productService.GetFilteredAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("Product.Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _productService.UpdateAsync(id, request);
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
            return StatusCode(500, new { message = _localizationService.GetString("ErrorUpdatingProduct"), error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("Product.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorDeletingProduct"), error = ex.Message });
        }
    }
}
