namespace DirectoryService.Domain.Entities;

public class DepartmentLocation(Guid departmentId, Guid locationId)
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid DepartmentId { get; private set; } = departmentId;

    public Guid LocationId { get; private set; } = locationId;
}