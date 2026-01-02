using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationService
{
    Task<Result<Guid, Error>> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken);
    Task<Result<Guid, Error>> UpdateLocationAsync(Guid departmentId, IEnumerable<Guid> locationIds, CancellationToken cancellationToken);
}