namespace DirectoryService.Contracts.Departments;

public record DepartmentDto
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }
    
    public string Name { get; init; }

    public string Identifier { get; init; }
    
    public IEnumerable<Guid> Positions { get; init; }
    
    public string Path { get; init; }

    public short Depth { get; init; }

    public int ChildrenCount { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}