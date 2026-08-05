using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentDescendants;

public record GetDepartmentDescendantIdsQuery(Guid DepartmentId) : IQuery;
