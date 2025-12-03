namespace DirectoryService.Domain.Entities;

public class DepartmentLocation
{
    public List<Guid> DepartmentIds { get; set; }

    public List<Guid> LocationIds { get; set; }
}