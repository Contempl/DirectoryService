namespace RabbitMqMessaging.IntegrationEvents.Files.Events;

// FS-14: Факт готовности файла для использования другими сервисами.
public sealed record FileReadyIntegrationEvent(
    Guid AssetId,
    string EntityType);
