using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.Delete;

public record DeleteDepartmentRequest(Guid DepartmentId) : ICommand;