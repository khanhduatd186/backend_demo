using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.DTOs.Language.Requests;
using backend.Application.Interfaces;
using backend.Domain.Language.Interfaces;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// Controller để quản lý localization/ngôn ngữ
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocalizationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly ILanguageRepository _languageRepository;

    public LocalizationController(
        IAuthService authService, 
        ILocalizationService localizationService,
        ILanguageService languageService,
        ILanguageRepository languageRepository)
    {
        _authService = authService;
        _localizationService = localizationService;
        _languageService = languageService;
        _languageRepository = languageRepository;
    }

    /// <summary>
    /// Lấy danh sách ngôn ngữ được hỗ trợ
    /// </summary>
    [HttpGet("supported-languages")]
    public async Task<IActionResult> GetSupportedLanguages()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        return Ok(new { languages = languages });
    }

    /// <summary>
    /// Lấy ngôn ngữ hiện tại
    /// </summary>
    [HttpGet("current")]
    public IActionResult GetCurrentLanguage()
    {
        var language = _localizationService.GetCurrentLanguage();
        return Ok(new { language = language });
    }

    /// <summary>
    /// Cập nhật ngôn ngữ cho user hiện tại (yêu cầu đăng nhập)
    /// </summary>
    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate language code từ database
            var language = await _languageRepository.GetByCodeAsync(request.Language);
            if (language == null || !language.IsActive)
            {
                return BadRequest(new { message = _localizationService.GetString("LanguageNotSupported") });
            }

            var updatedLanguage = await _authService.UpdateLanguageAsync(userId, request.Language);
            var message = _localizationService.GetString("LanguageChanged");
            
            return Ok(new 
            { 
                message = message,
                language = updatedLanguage 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy tất cả translations của một ngôn ngữ
    /// </summary>
    [HttpGet("translations/{languageCode}")]
    public async Task<IActionResult> GetTranslations(string languageCode)
    {
        var translations = await _languageService.GetTranslationsByLanguageCodeAsync(languageCode);
        return Ok(new { languageCode = languageCode, translations = translations });
    }

    /// <summary>
    /// Lấy chuỗi đã được dịch theo key
    /// </summary>
    [HttpGet("translate/{key}")]
    public IActionResult Translate(string key)
    {
        try
        {
            var translated = _localizationService.GetString(key);
            return Ok(new { key = key, value = translated });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizationService.GetString("TranslationKeyNotFound"), error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo mới translation (yêu cầu đăng nhập)
    /// </summary>
    [Authorize]
    [HttpPost("translations")]
    public async Task<IActionResult> CreateTranslation([FromBody] Application.DTOs.Language.Requests.CreateTranslationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _languageService.CreateTranslationAsync(request);
            return CreatedAtAction(nameof(GetTranslationById), new { id = response.Id }, response);
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
            return StatusCode(500, new { message = _localizationService.GetString("ErrorCreatingTranslation"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy translation theo ID (yêu cầu đăng nhập)
    /// </summary>
    [Authorize]
    [HttpGet("translations/{id}")]
    public async Task<IActionResult> GetTranslationById(Guid id)
    {
        try
        {
            var response = await _languageService.GetTranslationByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { message = _localizationService.GetString("TranslationNotFound") });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("AnErrorOccurred"), error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật translation (yêu cầu đăng nhập)
    /// </summary>
    [Authorize]
    [HttpPut("translations/{id}")]
    public async Task<IActionResult> UpdateTranslation(Guid id, [FromBody] Application.DTOs.Language.Requests.UpdateTranslationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _languageService.UpdateTranslationAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorUpdatingTranslation"), error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa translation (yêu cầu đăng nhập)
    /// </summary>
    [Authorize]
    [HttpDelete("translations/{id}")]
    public async Task<IActionResult> DeleteTranslation(Guid id)
    {
        try
        {
            await _languageService.DeleteTranslationAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localizationService.GetString("ErrorDeletingTranslation"), error = ex.Message });
        }
    }
}
