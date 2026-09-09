namespace RabbitMqMessaging.IntegrationEvents.Files.Events;

// FS-14: Факт неуспешного завершения обработки видео.
public sealed record VideoProcessingFailedIntegrationEvent(
    Guid AssetId,
    string EntityType);
