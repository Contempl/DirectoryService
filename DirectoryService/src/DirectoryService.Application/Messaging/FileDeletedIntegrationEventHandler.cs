using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using Microsoft.Extensions.Logging;
using RabbitMqMessaging.IntegrationEvents.Files.Events;

namespace DirectoryService.Application.Messaging;

public class FileDeletedIntegrationEventHandler
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<FileDeletedIntegrationEventHandler> _logger;

    public FileDeletedIntegrationEventHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<FileDeletedIntegrationEventHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    // FS-14: Wolverine обнаружит метод Handle по соглашению.
    public async Task Handle(
        FileDeletedIntegrationEvent message,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                message.EntityType,
                "location",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var location = await _locationRepository.GetByPhotoAssetIdAsync(
            message.AssetId,
            cancellationToken);

        // FS-14: Повторная доставка безопасна — фотография уже удалена или не привязана.
        if (location is null)
        {
            _logger.LogInformation(
                "Deleted file {AssetId} is not attached to a location",
                message.AssetId);

            return;
        }

        var removeResult = location.RemovePhoto();
        if (removeResult.IsFailure)
            throw new InvalidOperationException(removeResult.Error.Message);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            throw new InvalidOperationException(saveResult.Error.Message);

        _logger.LogInformation(
            "Removed deleted file {AssetId} from location {LocationId}",
            message.AssetId,
            location.Id);
    }
}