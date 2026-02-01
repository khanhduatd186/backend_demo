using AutoMapper;
using backend.Application.DTOs.Language.Requests;
using backend.Application.DTOs.Language.Responses;
using backend.Application.Interfaces;
using backend.Domain.Common;
using backend.Domain.Language.Entities;
using backend.Domain.Language.Interfaces;

namespace backend.Application.Services;

/// <summary>
/// Service để quản lý languages và translations
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _languageRepository;
    private readonly ITranslationRepository _translationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    public LanguageService(
        ILanguageRepository languageRepository,
        ITranslationRepository translationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILocalizationService localizationService)
    {
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    public async Task<IEnumerable<LanguageResponse>> GetAllLanguagesAsync()
    {
        var languages = await _languageRepository.GetActiveLanguagesAsync();
        return _mapper.Map<IEnumerable<LanguageResponse>>(languages);
    }

    public async Task<LanguageResponse?> GetLanguageByCodeAsync(string code)
    {
        var language = await _languageRepository.GetByCodeAsync(code);
        return language == null ? null : _mapper.Map<LanguageResponse>(language);
    }

    public async Task<Dictionary<string, string>> GetTranslationsByLanguageCodeAsync(string languageCode)
    {
        return await _translationRepository.GetAllTranslationsByLanguageCodeAsync(languageCode);
    }

    public async Task<TranslationResponse?> GetTranslationAsync(string key, string languageCode)
    {
        var translation = await _translationRepository.GetByKeyAndLanguageCodeAsync(key, languageCode);
        if (translation == null)
        {
            return null;
        }

        return new TranslationResponse
        {
            Id = translation.Id,
            Key = translation.Key,
            Value = translation.Value,
            LanguageCode = languageCode,
            CreatedAt = translation.CreatedAt
        };
    }

    public async Task<TranslationResponse> CreateTranslationAsync(CreateTranslationRequest request)
    {
        // Validate language code
        var language = await _languageRepository.GetByCodeAsync(request.LanguageCode);
        if (language == null || !language.IsActive)
        {
            throw new KeyNotFoundException(_localizationService.GetString("LanguageNotSupported"));
        }

        // Check if translation key already exists for this language
        var existing = await _translationRepository.GetByKeyAndLanguageIdAsync(request.Key, language.Id);
        if (existing != null)
        {
            throw new InvalidOperationException(_localizationService.GetString("TranslationKeyExists", request.Key, request.LanguageCode));
        }

        // Create new translation
        var translation = new Translation
        {
            Key = request.Key,
            Value = request.Value,
            LanguageId = language.Id
        };

        await _translationRepository.AddAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache để reload translations
        _localizationService.SetLanguage(request.LanguageCode);

        return new TranslationResponse
        {
            Id = translation.Id,
            Key = translation.Key,
            Value = translation.Value,
            LanguageCode = request.LanguageCode,
            CreatedAt = translation.CreatedAt
        };
    }

    public async Task<TranslationResponse> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request)
    {
        var translation = await _translationRepository.GetByIdAsync(id);
        if (translation == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("TranslationNotFound"));
        }

        // Update value
        translation.Value = request.Value;
        translation.UpdatedAt = DateTime.UtcNow;

        await _translationRepository.UpdateAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // Get language code for response
        var language = await _languageRepository.GetByIdAsync(translation.LanguageId);
        var languageCode = language?.Code ?? string.Empty;

        // Clear cache để reload translations
        if (!string.IsNullOrEmpty(languageCode))
        {
            _localizationService.SetLanguage(languageCode);
        }

        return new TranslationResponse
        {
            Id = translation.Id,
            Key = translation.Key,
            Value = translation.Value,
            LanguageCode = languageCode,
            CreatedAt = translation.CreatedAt
        };
    }

    public async Task<TranslationResponse?> GetTranslationByIdAsync(Guid id)
    {
        var translation = await _translationRepository.GetByIdAsync(id);
        if (translation == null)
        {
            return null;
        }

        // Get language code
        var language = await _languageRepository.GetByIdAsync(translation.LanguageId);
        var languageCode = language?.Code ?? string.Empty;

        return new TranslationResponse
        {
            Id = translation.Id,
            Key = translation.Key,
            Value = translation.Value,
            LanguageCode = languageCode,
            CreatedAt = translation.CreatedAt
        };
    }

    public async Task DeleteTranslationAsync(Guid id)
    {
        var translation = await _translationRepository.GetByIdAsync(id);
        if (translation == null)
        {
            throw new KeyNotFoundException(_localizationService.GetString("TranslationNotFound"));
        }

        await _translationRepository.DeleteAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache để reload translations
        var language = await _languageRepository.GetByIdAsync(translation.LanguageId);
        if (language != null)
        {
            _localizationService.SetLanguage(language.Code);
        }
    }
}
