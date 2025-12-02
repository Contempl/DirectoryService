using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities;

public class Department
{
    private List<Department> _children = [];
    private List<Location> _locations = [];
    private Department(Guid id, 
        Name name, 
        Guid? parentId, 
        Identifier identifier, 
        Path path,
        short depth,
        bool isActive,
        IEnumerable<Location> locations,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Name = name;
        Identifier = identifier;
        Path = path;
        Id = Guid.NewGuid();
    }
    
    public Guid Id { get; private set; }

    public Guid? ParentId { get; private set; }
    
    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public List<Location> Locations => _locations;
    
    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public int ChildrenCount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Department> Create(
        Name name,
        Identifier identifier,
        Guid? parentId,
        short depth,
        Path path,
        IEnumerable<Location> locations)
    {
        if (!locations.Any())
            return Result.Failure<Department>("No locations specified");
        
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        return new Department(
            id, 
            name, 
            parentId, 
            identifier,
            path, 
            depth, 
            true,
            locations, 
            createdAt, 
            null);
    }
}

public record Identifier
{
    private Identifier(string identifier)
    {
        Value = identifier;
    }
    public string Value { get; }

    public static Result<Identifier> Create(Guid id, string value)
    {
        foreach (var letter in value.ToCharArray())
        {
            if (!char.IsAsciiLetterOrDigit(letter))
                return Result.Failure<Identifier>("Value must contain only Latin characters and numbers");
        }
        
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Identifier>("Identifier must be not empty and between 150 characters");

        return new Identifier(value);
    }
}

public record Name
{
    private Name(string name)
    {
        Value = name;
    }
    public string Value { get; }

    public static Result<Name> Create(Guid id, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Name>("Name must be not empty and between 150 characters");
        return new Name(value);
    }
}

public record Path
{
    private Path(string path)
    {
        Value = path;
    }
    public string Value { get; }

    public static Result<Path> Create(Guid id, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Path>("Path cannot be empty and between 150 characters");
        
        return new Path(value);
    }
}

