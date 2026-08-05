using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Entities;

public static class DepartmentErrors
{
    public static Error ActivityChangeForDeleted() => Error.Conflict(
        "department.activity.deleted",
        "Deleted department activity cannot be changed.");
}
