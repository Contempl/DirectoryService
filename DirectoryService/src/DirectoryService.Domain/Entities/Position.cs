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
        bool isActive,
        DepartmentPosition departmentPositions,
        DateTime createdAt, 
        DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }


    public static Result<Position> Create(Name name, string description, DepartmentPosition departmentPositions)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length > 1000)
            return Result.Failure<Position>("Description cannot be empty and must be less than 1000 characters");
        
        
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        return new Position(id, name, description, true, departmentPositions, createdAt, null);
    }
}