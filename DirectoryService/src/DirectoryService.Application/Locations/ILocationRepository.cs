using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid, Errors>> CreateAsync(Location location, CancellationToken cancellationToken = default);
    
    Task<bool> CheckIfLocationsExistAsync(List<Guid> locationIds, CancellationToken cancellationToken = default);
    
    Task<Result<Location, Error>> GetLocationByIdAsync(Guid locationId, CancellationToken cancellationToken = default);
}