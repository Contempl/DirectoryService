using Wolverine;
using Wolverine.RabbitMQ;
using RabbitMqMessaging.IntegrationEvents.Files;
using RabbitMqMessaging.IntegrationEvents.Files.Events;

namespace RabbitMqMessaging;

public static class RabbitMqMessagingExtensions
{
    public static void ConfigureRabbitMq(
        this WolverineOptions options,
        RabbitMqOptions rabbitMqOptions)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(rabbitMqOptions);

        if (string.IsNullOrWhiteSpace(rabbitMqOptions.Host))
            throw new InvalidOperationException("RabbitMq:Host is required.");

        if (string.IsNullOrWhiteSpace(rabbitMqOptions.UserName))
            throw new InvalidOperationException("RabbitMq:UserName is required.");

        if (string.IsNullOrWhiteSpace(rabbitMqOptions.Password))
            throw new InvalidOperationException("RabbitMq:Password is required.");

        if (rabbitMqOptions.Port is < 1 or > 65535)
            throw new InvalidOperationException("RabbitMq:Port must be between 1 and 65535.");

        options
            .UseRabbitMq(connection =>
            {
                connection.HostName = rabbitMqOptions.Host;
                connection.Port = rabbitMqOptions.Port;
                connection.UserName = rabbitMqOptions.UserName;
                connection.Password = rabbitMqOptions.Password;
                connection.VirtualHost = rabbitMqOptions.VirtualHost;
            })
            .AutoProvision()
            .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Topic;
                exchange.IsDurable = true;
            });
        
        options.PublishMessagesToRabbitMqExchange<FileReadyIntegrationEvent>(
            FileEventsRouting.EXCHANGE,
            message => FileEventsRouting.RoutingKeys.FileReady(message.EntityType))
            .UseDurableOutbox();

        options.PublishMessagesToRabbitMqExchange<FileDeletedIntegrationEvent>(
            FileEventsRouting.EXCHANGE,
            message => FileEventsRouting.RoutingKeys.FileDeleted(message.EntityType))
            .UseDurableOutbox();

        options.PublishMessagesToRabbitMqExchange<VideoProcessingCompletedIntegrationEvent>(
            FileEventsRouting.EXCHANGE,
            message => FileEventsRouting.RoutingKeys.VideoProcessingCompleted(message.EntityType))
            .UseDurableOutbox();

        options.PublishMessagesToRabbitMqExchange<VideoProcessingFailedIntegrationEvent>(
            FileEventsRouting.EXCHANGE,
            message => FileEventsRouting.RoutingKeys.VideoProcessingFailed(message.EntityType))
            .UseDurableOutbox();
    }
}