using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default);
}