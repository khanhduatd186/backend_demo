using Microsoft.EntityFrameworkCore;
using backend.Domain.Example.Entities;
using backend.Domain.Example.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Common;

namespace backend.Infrastructure.Repositories.Example;

/// <summary>
/// Repository implementation cho ExampleEntity
/// </summary>
public class ExampleRepository : Repository<Domain.Example.Entities.ExampleEntity>, IExampleRepository
{
    public ExampleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Example.Entities.ExampleEntity?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(x => x.Name == name && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Domain.Example.Entities.ExampleEntity>> GetActiveAsync()
    {
        return await _dbSet
            .Where(x => x.IsActive && !x.IsDeleted)
            .ToListAsync();
    }
}
