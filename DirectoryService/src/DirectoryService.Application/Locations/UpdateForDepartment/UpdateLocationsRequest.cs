using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.UpdateForDepartment;

public record UpdateLocationsRequest(Guid DepartmentId, IEnumerable<Guid> LocationIds) : ICommand;