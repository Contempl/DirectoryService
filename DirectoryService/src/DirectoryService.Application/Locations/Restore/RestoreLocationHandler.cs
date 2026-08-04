using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Restore;

public class RestoreLocationHandler : ICommandHandler<Guid, RestoreLocationRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RestoreLocationHandler> _logger;

    public RestoreLocationHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<RestoreLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        RestoreLocationRequest request,
        CancellationToken cancellationToken)
    {
        var locationResult = await _locationRepository.GetDeletedLocationByIdAsync(
            request.LocationId,
            cancellationToken);

        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var restoreResult = locationResult.Value.Restore();
        if (restoreResult.IsFailure)
            return restoreResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            _logger.LogError("Failed to save restored location {LocationId}.", request.LocationId);
            return saveResult.Error.ToErrors();
        }

        return request.LocationId;
    }
}
