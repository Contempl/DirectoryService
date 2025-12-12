using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken = default);
}