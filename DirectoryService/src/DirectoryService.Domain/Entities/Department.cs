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
    
    public DateTime? DeletedAt { get; private set; }

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
            path = $"{parent.Path.Value}.{identifier.Value}";
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
    public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> newLocations)
    {
        var locations = newLocations.ToList();
        var newLocationIds = locations.Select(x => x.Id).ToList();
        var toRemove = _locations.Where(l => !newLocationIds.Contains(l.LocationId)).ToList();
        foreach (var old in toRemove)
            _locations.Remove(old);

        var remainingIds = _locations.Select(l => l.LocationId).ToList();
        var toAdd = locations.Where(l => !remainingIds.Contains(l.LocationId));
        _locations.AddRange(toAdd);
    
        UpdatedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SoftDelete()
    {
        if (IsActive is false)
            return GeneralErrors.ValueIsInvalid("Department is already deleted.");
        
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }
    
    public Result<Path, Error> ChangePathAfterSoftDelete()
    {
        var pathArr = Path.Value.Split('.');
        var subArr = pathArr[..^1];
        var subpath = string.Join('.', subArr.Select(x => x));
        var pathWithDeletedPrefix = string.Concat("deleted_", pathArr.Last());
        
        var newPath = string.Join('.', subpath, pathWithDeletedPrefix);
        
        var pathResult = Path.Create(newPath);
        if (pathResult.IsFailure)
            return pathResult.Error;
        
        return pathResult.Value;
    }
}