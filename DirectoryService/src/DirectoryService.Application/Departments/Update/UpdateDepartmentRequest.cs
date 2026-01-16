using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Update;

public record UpdateDepartmentRequest(Guid DepartmentId, Guid? ParentId) : ICommand;