using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.VO;
using Path = DirectoryService.Domain.Entities.VO.Path;

namespace DirectoryService.Domain.Entities;

public class Department
{
    public Department() { }
    
    private readonly List<DepartmentLocation> _locations = [];
    private readonly List<DepartmentPosition> _positions = [];
    private Department(Guid id, 
        Name name, 
        Guid? parentId, 
        Identifier identifier, 
        Path path,
        IEnumerable<Position> departmentPositions,
        short depth,
        bool isActive,
        IEnumerable<Location> locations,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = Guid.NewGuid();
        Name = name;
        ParentId = parentId;
        Identifier = identifier;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    
    public Guid Id { get; private set; }

    public Guid? ParentId { get; private set; }
    
    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    
    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public int ChildrenCount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Department> Create(
        Name name,
        Identifier identifier,
        IEnumerable<Position> departmentPositions,
        Guid? parentId,
        short depth,
        Path path,
        IEnumerable<Location> departmentLocations)
    {
        if (!departmentLocations.Any())
            return Result.Failure<Department>("No locations specified");

        if (!departmentPositions.Any())
            return Result.Failure<Department>("No positions specified");
        
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        return new Department(
            id, 
            name, 
            parentId, 
            identifier,
            path, 
            departmentPositions,
            depth, 
            true,
            departmentLocations, 
            createdAt, 
            null);
    }
}