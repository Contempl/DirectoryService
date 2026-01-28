namespace DirectoryService.Application.Departments.Queries.ChildrenDepartments;

public record GetChildrenQuery(Guid ParentId, GetChildrenRequest Request);