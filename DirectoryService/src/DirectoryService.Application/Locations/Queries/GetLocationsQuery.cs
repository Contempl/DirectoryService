namespace DirectoryService.Application.Locations.Queries;

public record GetLocationsQuery(Guid[]? DepartmentIds, string? Search, bool IsActive = true, int Page = 1, int PageSize = 20);