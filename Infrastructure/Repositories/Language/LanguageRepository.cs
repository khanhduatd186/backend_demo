using Microsoft.EntityFrameworkCore;
using backend.Domain.Language.Entities;
using backend.Domain.Language.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Language;

/// <summary>
/// Repository implementation cho Language entity
/// </summary>
public class LanguageRepository : Repository<Domain.Language.Entities.Language>, ILanguageRepository
{
    public LanguageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Language.Entities.Language?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Where(x => x.Code == code && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Language.Entities.Language?> GetDefaultAsync()
    {
        return await _dbSet
            .Where(x => x.IsDefault && x.IsActive && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Domain.Language.Entities.Language>> GetActiveLanguagesAsync()
    {
        return await _dbSet
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbSet
            .AnyAsync(x => x.Code == code && !x.IsDeleted);
    }
}
