using Shared.Kernel;

namespace DirectoryService.Application.Departments;

public static class DepartmentMoveErrors
{
    public static Error Cycle() => Error.Validation(
        "department.move.cycle",
        "A department cannot be moved into itself or one of its descendants.",
        "parentId");

    public static Error ParentDeleted() => Error.Validation(
        "department.move.parent_deleted",
        "The selected parent department is deleted.",
        "parentId");

    public static Error DepartmentNotFound(Guid departmentId) => Error.NotFound(
        "department.move.not_found",
        $"Department {departmentId} was not found.");
}
