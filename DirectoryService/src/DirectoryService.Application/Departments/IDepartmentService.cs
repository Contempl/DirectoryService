using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentService
{
    Task<Result<Guid, Error>> CreateDepartmentAsync(CreateDepartmentRequest departmentDto, CancellationToken cancellationToken);
}