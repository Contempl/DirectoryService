namespace DirectoryService.Contracts.Locations;

public record LocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public AddressDto Address { get; init; }

    public string Timezone { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

}