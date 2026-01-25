using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.Update;

public record UpdateDepartmentRequest(Guid DepartmentId, Guid? ParentId) : ICommand;