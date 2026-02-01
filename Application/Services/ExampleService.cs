using AutoMapper;
using backend.Application.DTOs.Example.Requests;
using backend.Application.DTOs.Example.Responses;
using backend.Application.Interfaces;
using backend.Domain.Example.Entities;
using backend.Domain.Example.Interfaces;
using backend.Domain.Common;

namespace backend.Application.Services;

public class ExampleService : IExampleService
{
    private readonly IExampleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExampleService(IExampleRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExampleResponse> CreateAsync(CreateExampleRequest request)
    {
        var entity = _mapper.Map<Domain.Example.Entities.ExampleEntity>(request);
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ExampleResponse>(entity);
    }

    public async Task<ExampleResponse?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<ExampleResponse>(entity);
    }

    public async Task<IEnumerable<ExampleResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ExampleResponse>>(entities);
    }

    public async Task<ExampleResponse> UpdateAsync(Guid id, CreateExampleRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entity with id {id} not found");
        }

        _mapper.Map(request, entity);
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExampleResponse>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entity with id {id} not found");
        }

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
