namespace DirectoryService.Application.Positions.Queries;

public record GetPositionsQuery(Guid[]? PositionIds, string? Search, bool IsActive = true, int Page = 1, int PageSize = 20);