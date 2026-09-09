namespace RabbitMqMessaging.IntegrationEvents.Files;

// FS-14: Общие имена exchange и routing keys для publisher и consumers.
public static class FileEventsRouting
{
    public const string EXCHANGE = "file.events";

    public static class RoutingKeys
    {
        public static string AllFor(string entityType) =>
            $"*.*.{NormalizeSegment(entityType, nameof(entityType))}";

        public static string FileReady(string entityType) =>
            $"file.ready.{NormalizeSegment(entityType, nameof(entityType))}";

        public static string FileDeleted(string entityType) =>
            $"file.deleted.{NormalizeSegment(entityType, nameof(entityType))}";

        public static string VideoProcessingCompleted(string entityType) =>
            $"video.completed.{NormalizeSegment(entityType, nameof(entityType))}";

        public static string VideoProcessingFailed(string entityType) =>
            $"video.failed.{NormalizeSegment(entityType, nameof(entityType))}";
    }

    private static string NormalizeSegment(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Any(character =>
                char.IsWhiteSpace(character) || character is '.' or '*' or '#'))
        {
            throw new ArgumentException(
                "A routing key segment cannot contain whitespace, '.', '*' or '#'.",
                paramName);
        }

        return normalizedValue;
    }
}
