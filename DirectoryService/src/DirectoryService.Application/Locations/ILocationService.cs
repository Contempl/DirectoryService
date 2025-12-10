using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations;

public interface ILocationService
{
    Task<Guid> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken);
}