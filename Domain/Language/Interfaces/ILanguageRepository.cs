using backend.Domain.Common;
using LanguageEntity = backend.Domain.Language.Entities.Language;

namespace backend.Domain.Language.Interfaces;

/// <summary>
/// Repository interface cho Language entity
/// </summary>
public interface ILanguageRepository : IRepository<LanguageEntity>
{
    Task<LanguageEntity?> GetByCodeAsync(string code);
    Task<LanguageEntity?> GetDefaultAsync();
    Task<IEnumerable<LanguageEntity>> GetActiveLanguagesAsync();
    Task<bool> ExistsByCodeAsync(string code);
}
