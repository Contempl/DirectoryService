using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, Address Address, Timezone Timezone);