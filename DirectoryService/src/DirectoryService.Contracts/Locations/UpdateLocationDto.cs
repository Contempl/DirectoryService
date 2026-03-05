namespace DirectoryService.Contracts.Locations;

public record UpdateLocationDto(string Name, string City, string Street, string House, string? Apartment, string Timezone);
