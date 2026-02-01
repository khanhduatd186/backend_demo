using Microsoft.Extensions.Caching.Memory;
using backend.Domain.Language.Interfaces;
using backend.Domain.User.Interfaces;
using backend.Application.Interfaces;
using System.Security.Claims;

namespace backend.Application.Services;

/// <summary>
/// Service để quản lý localization/ngôn ngữ - đọc từ database
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ITranslationRepository _translationRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "translations_";
    private const int CacheExpirationMinutes = 60;

    public LocalizationService(
        ITranslationRepository translationRepository,
        ILanguageRepository languageRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
    {
        _translationRepository = translationRepository;
        _languageRepository = languageRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public string GetString(string key, params object[] args)
    {
        var languageCode = GetCurrentLanguage();
        var cacheKey = $"{CacheKeyPrefix}{languageCode}";
        
        // Lấy translations từ cache hoặc database
        Dictionary<string, string> translations;
        if (!_cache.TryGetValue(cacheKey, out translations))
        {
            translations = _translationRepository.GetAllTranslationsByLanguageCodeAsync(languageCode).Result;
            _cache.Set(cacheKey, translations, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }

        // Lấy giá trị dịch
        var value = translations.TryGetValue(key, out var translatedValue) 
            ? translatedValue 
            : key; // Nếu không tìm thấy, trả về key

        if (args != null && args.Length > 0)
        {
            return string.Format(value, args);
        }
        return value;
    }

    public string GetCurrentLanguage()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return GetDefaultLanguageCode();
        }

        // ƯU TIÊN 1: Lấy từ query string (override tất cả)
        var queryLang = httpContext.Request.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryLang))
        {
            var lang = _languageRepository.GetByCodeAsync(queryLang).Result;
            if (lang != null && lang.IsActive)
            {
                return queryLang;
            }
        }

        // ƯU TIÊN 2: Nếu user đã đăng nhập, lấy từ database (user preference)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var user = _userRepository.GetByIdAsync(userId).Result;
                    if (user != null && !string.IsNullOrEmpty(user.Language))
                    {
                        var lang = _languageRepository.GetByCodeAsync(user.Language).Result;
                        if (lang != null && lang.IsActive)
                        {
                            return user.Language;
                        }
                    }
                }
                catch
                {
                    // Nếu lỗi, tiếp tục với các phương thức khác
                }
            }
        }

        // ƯU TIÊN 3: Lấy từ Items (đã được middleware set)
        var userLang = httpContext.Items["UserLanguage"]?.ToString();
        if (!string.IsNullOrEmpty(userLang))
        {
            var lang = _languageRepository.GetByCodeAsync(userLang).Result;
            if (lang != null && lang.IsActive)
            {
                return userLang;
            }
        }

        // ƯU TIÊN 4: Lấy từ header Accept-Language (chỉ khi chưa đăng nhập)
        var headerLang = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerLang))
        {
            var languages = headerLang.Split(',')
                .Select(l => l.Split(';')[0].Trim().Split('-')[0].ToLower())
                .ToList();
            
            foreach (var langCode in languages)
            {
                var lang = _languageRepository.GetByCodeAsync(langCode).Result;
                if (lang != null && lang.IsActive)
                {
                    return langCode;
                }
            }
        }

        return GetDefaultLanguageCode();
    }

    public void SetLanguage(string languageCode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var lang = _languageRepository.GetByCodeAsync(languageCode).Result;
            if (lang != null && lang.IsActive)
            {
                httpContext.Items["CurrentLanguage"] = languageCode;
                // Clear cache để reload translations
                _cache.Remove($"{CacheKeyPrefix}{languageCode}");
            }
        }
    }

    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        var languages = await _languageRepository.GetActiveLanguagesAsync();
        return languages.Select(l => l.Code);
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return GetSupportedLanguagesAsync().Result;
    }

    private string GetDefaultLanguageCode()
    {
        var defaultLang = _languageRepository.GetDefaultAsync().Result;
        return defaultLang?.Code ?? "vi";
    }
}
