using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, string City, string Street, string House, string? Apartment, string Timezone);