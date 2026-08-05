using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.ToggleActivity;

public record ToggleDepartmentActivityRequest(
    Guid DepartmentId,
    bool IsActive) : ICommand;