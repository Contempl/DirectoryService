using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
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
        short depth,
        List<DepartmentLocation> locations)
    {
        Id = Guid.NewGuid();
        Name = name;
        ParentId = parentId;
        Identifier = identifier;
        Path = path;
        Depth = depth;
        _locations = locations;
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

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; } = DateTime.UtcNow;

    public static Result<Department, Error> Create(
        Name name,
        Identifier identifier,
        Guid? parentId,
        Department? parent,
        List<DepartmentLocation> locations)
    {
        
        var id = Guid.NewGuid();
        short depth;
        string path;
        
        if (parent is null)
        {
            path = identifier.Value;
            depth = 0;
            parentId = null;
        }
        else
        {
            path = $"{parent.Path}.{identifier}";
            depth = (short)(parent.Depth + 1);
            parentId = parent.Id; 
        }

        var pathVo = Path.Create(path);
        if (pathVo.IsFailure)
            return pathVo.Error;
        
        return new Department(
            id, 
            name, 
            parentId, 
            identifier,
            pathVo.Value,
            depth, 
            locations);
    }
    
    public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> newLocationIds)
    {
        var locationList = newLocationIds.ToList();

        if (locationList.Count == 0)
            return GeneralErrors.ValueIsRequired("locationId");

        _locations.Clear();

        _locations.AddRange(locationList);

        return UnitResult.Success<Error>();
    }
}