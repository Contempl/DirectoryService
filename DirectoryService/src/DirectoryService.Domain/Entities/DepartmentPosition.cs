namespace DirectoryService.Domain.Entities;

public class DepartmentPosition
{
    public List<Guid> DepartmentIds { get; set; }

    public List<Guid> PositionIds { get; set; }
}