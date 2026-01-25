namespace DirectoryService.Domain.Entities;

public class DepartmentLocation(Guid departmentId, Guid locationId)
{
    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; } = departmentId;

    public Guid LocationId { get; private set; } = locationId;

    public static DepartmentLocation Create(Guid departmentId, Guid locationId)
    {
        return new DepartmentLocation(departmentId, locationId);
    }
}