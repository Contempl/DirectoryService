namespace RabbitMqMessaging.IntegrationEvents.Files.Events;

// FS-14: Факт успешного завершения обработки видео.
public sealed record VideoProcessingCompletedIntegrationEvent(
    Guid AssetId,
    string EntityType);
