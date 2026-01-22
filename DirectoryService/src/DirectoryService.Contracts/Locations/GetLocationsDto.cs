using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.Contracts.Locations;

public record GetLocationsDto(List<LocationDto> Locations, long TotalCount);

public record LocationDto
{
    public Guid Id { get; init; }

    public Name Name { get; init; }

    public Address Address { get; init; }

    public Timezone Timezone { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

}