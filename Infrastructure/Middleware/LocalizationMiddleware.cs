using Microsoft.AspNetCore.Localization;
using backend.Domain.User.Interfaces;
using backend.Domain.Language.Interfaces;

namespace backend.Infrastructure.Middleware;

/// <summary>
/// Middleware để xử lý localization dựa trên user preference hoặc request - đọc từ database
/// </summary>
public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LocalizationMiddleware> _logger;

    public LocalizationMiddleware(RequestDelegate next, ILogger<LocalizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserRepository userRepository,
        ILanguageRepository languageRepository)
    {
        var defaultLanguage = await languageRepository.GetDefaultAsync();
        var defaultCulture = defaultLanguage?.Code ?? "vi";
        var culture = defaultCulture;

        // 1. Kiểm tra query string
        var queryLang = context.Request.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryLang))
        {
            var lang = await languageRepository.GetByCodeAsync(queryLang);
            if (lang != null && lang.IsActive)
            {
                culture = queryLang;
            }
        }
        // 2. Kiểm tra header Accept-Language
        else if (context.Request.Headers.ContainsKey("Accept-Language"))
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            var languages = acceptLanguage.Split(',')
                .Select(l => l.Split(';')[0].Trim().Split('-')[0].ToLower())
                .ToList();
            
            foreach (var langCode in languages)
            {
                var lang = await languageRepository.GetByCodeAsync(langCode);
                if (lang != null && lang.IsActive)
                {
                    culture = langCode;
                    break;
                }
            }
        }
        // 3. Nếu user đã đăng nhập, lấy từ database
        else if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var user = await userRepository.GetByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.Language))
                    {
                        var lang = await languageRepository.GetByCodeAsync(user.Language);
                        if (lang != null && lang.IsActive)
                        {
                            culture = user.Language;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get user language preference");
                }
            }
        }

        // Set culture cho request
        context.Items["CurrentLanguage"] = culture;
        context.Items["UserLanguage"] = culture;

        // Set request culture
        var requestCulture = new RequestCulture(culture);
        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));

        await _next(context);
    }
}
