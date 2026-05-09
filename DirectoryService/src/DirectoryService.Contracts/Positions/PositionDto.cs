namespace DirectoryService.Contracts.Positions;

public record PositionDto
{
    public Guid Id { get; init; }
    
    public string Name { get; init; }

    public string Description { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}