using backend.Application.DTOs.Language.Requests;
using backend.Application.DTOs.Language.Responses;

namespace backend.Application.Interfaces;

/// <summary>
/// Service interface để quản lý languages và translations
/// </summary>
public interface ILanguageService
{
    Task<IEnumerable<LanguageResponse>> GetAllLanguagesAsync();
    Task<LanguageResponse?> GetLanguageByCodeAsync(string code);
    Task<Dictionary<string, string>> GetTranslationsByLanguageCodeAsync(string languageCode);
    Task<TranslationResponse?> GetTranslationAsync(string key, string languageCode);
    Task<TranslationResponse> CreateTranslationAsync(CreateTranslationRequest request);
    Task<TranslationResponse> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request);
    Task<TranslationResponse?> GetTranslationByIdAsync(Guid id);
    Task DeleteTranslationAsync(Guid id);
}
