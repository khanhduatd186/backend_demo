using Microsoft.EntityFrameworkCore;
using backend.Domain.Language.Entities;
using backend.Domain.Language.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Language;

/// <summary>
/// Repository implementation cho Translation entity
/// </summary>
public class TranslationRepository : Repository<Domain.Language.Entities.Translation>, ITranslationRepository
{
    public TranslationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Language.Entities.Translation?> GetByKeyAndLanguageIdAsync(string key, Guid languageId)
    {
        return await _dbSet
            .Where(x => x.Key == key && x.LanguageId == languageId && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Language.Entities.Translation?> GetByKeyAndLanguageCodeAsync(string key, string languageCode)
    {
        return await _dbSet
            .Where(x => x.Key == key && x.Language.Code == languageCode && !x.IsDeleted && !x.Language.IsDeleted)
            .Include(x => x.Language)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<string, string>> GetAllTranslationsByLanguageCodeAsync(string languageCode)
    {
        var translations = await _dbSet
            .Where(x => x.Language.Code == languageCode && !x.IsDeleted && !x.Language.IsDeleted)
            .Include(x => x.Language)
            .ToListAsync();

        return translations.ToDictionary(t => t.Key, t => t.Value);
    }

    public async Task<bool> ExistsByKeyAndLanguageIdAsync(string key, Guid languageId)
    {
        return await _dbSet
            .AnyAsync(x => x.Key == key && x.LanguageId == languageId && !x.IsDeleted);
    }
}
