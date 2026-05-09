namespace DirectoryService.Application.Departments.Queries.GetListOfDepartments;

public record GetDepartmentsQuery(
    Guid[]? LocationsIds,
    string? Search,
    bool? IsActive,
    Guid? ParentId,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 20);