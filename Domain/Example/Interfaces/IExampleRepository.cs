using backend.Domain.Common;
using ExampleEntity = backend.Domain.Example.Entities.ExampleEntity;

namespace backend.Domain.Example.Interfaces;

/// <summary>
/// Repository interface cho ExampleEntity
/// </summary>
public interface IExampleRepository : IRepository<ExampleEntity>
{
    Task<ExampleEntity?> GetByNameAsync(string name);
    Task<IEnumerable<ExampleEntity>> GetActiveAsync();
}
