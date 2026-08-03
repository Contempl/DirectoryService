namespace DirectoryService.Application.Positions.Queries;

public record GetPositionsQuery(Guid[]? DepartmentIds, string? Search, bool IsActive = true, int Page = 1, int PageSize = 20);
