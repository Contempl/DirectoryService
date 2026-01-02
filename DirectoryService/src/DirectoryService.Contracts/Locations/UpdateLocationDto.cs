namespace DirectoryService.Contracts.Locations;

public record UpdateLocationDto (Guid departmentId, IEnumerable<Guid> locationIds);