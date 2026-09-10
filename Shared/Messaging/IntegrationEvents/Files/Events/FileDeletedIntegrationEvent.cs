namespace RabbitMqMessaging.IntegrationEvents.Files.Events;

// FS-14: Факт удаления файла в File Service.
public sealed record FileDeletedIntegrationEvent(
    Guid AssetId,
    string EntityType);
