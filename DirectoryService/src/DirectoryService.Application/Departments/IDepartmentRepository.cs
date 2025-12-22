using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken = default);
    
    Task<Result<Department, Errors>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CheckIfDepartmentsExistAsync(List<Guid> departmentIds, CancellationToken cancellationToken = default);
}