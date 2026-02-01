using backend.Domain.Common;
using TranslationEntity = backend.Domain.Language.Entities.Translation;

namespace backend.Domain.Language.Interfaces;

/// <summary>
/// Repository interface cho Translation entity
/// </summary>
public interface ITranslationRepository : IRepository<TranslationEntity>
{
    Task<TranslationEntity?> GetByKeyAndLanguageIdAsync(string key, Guid languageId);
    Task<TranslationEntity?> GetByKeyAndLanguageCodeAsync(string key, string languageCode);
    Task<Dictionary<string, string>> GetAllTranslationsByLanguageCodeAsync(string languageCode);
    Task<bool> ExistsByKeyAndLanguageIdAsync(string key, Guid languageId);
}
