namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public record GetChildrenQuery(Guid ParentId, GetChildrenRequest Request);