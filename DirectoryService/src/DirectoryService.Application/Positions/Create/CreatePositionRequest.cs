using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Create;

public record CreatePositionRequest(string Name, string Description, List<Guid> DepartmentIds) : ICommand;