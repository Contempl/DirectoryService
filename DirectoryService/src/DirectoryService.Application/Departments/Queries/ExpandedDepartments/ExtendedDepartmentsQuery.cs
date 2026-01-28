namespace DirectoryService.Application.Departments.Queries.ExpandedDepartments;

public record ExtendedDepartmentsQuery(int Page = 1, int Size = 20, int Prefetch = 3);