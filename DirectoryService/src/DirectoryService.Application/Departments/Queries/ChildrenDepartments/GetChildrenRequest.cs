namespace DirectoryService.Application.Departments.Queries.ChildrenDepartments;

public record GetChildrenRequest(int Page = 1, int Size = 20);