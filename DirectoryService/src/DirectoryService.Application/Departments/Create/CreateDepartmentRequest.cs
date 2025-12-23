using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentRequest(string Name, string Identifier, Guid? ParentId, List<Guid> LocationIds) : ICommand;