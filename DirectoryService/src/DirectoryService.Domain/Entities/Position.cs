using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.Domain.Entities;

public class Position
{
    public Position() { }
    
    private List<DepartmentPosition> _departmentPositions = [];
    private Position(Guid id, 
        Name name, 
        string description,
        List<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        _departmentPositions = departmentPositions;
    }
    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; } = DateTime.UtcNow;


    public static Result<Position> Create(Name name, string description, List<DepartmentPosition> departmentPositions)
    {
        var id = Guid.NewGuid();
        return new Position(id, name, description, departmentPositions);
    }
}