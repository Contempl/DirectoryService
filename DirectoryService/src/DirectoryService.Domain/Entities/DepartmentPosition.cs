namespace DirectoryService.Domain.Entities;

public class DepartmentPosition(Guid departmentId, Guid positionId)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public Guid DepartmentId { get; private set; } = departmentId;

    public Guid PositionId { get; private set;  } = positionId;
}