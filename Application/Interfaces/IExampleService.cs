using backend.Application.DTOs.Example.Requests;
using backend.Application.DTOs.Example.Responses;

namespace backend.Application.Interfaces;

public interface IExampleService
{
    Task<ExampleResponse> CreateAsync(CreateExampleRequest request);
    Task<ExampleResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExampleResponse>> GetAllAsync();
    Task<ExampleResponse> UpdateAsync(Guid id, CreateExampleRequest request);
    Task DeleteAsync(Guid id);
}
