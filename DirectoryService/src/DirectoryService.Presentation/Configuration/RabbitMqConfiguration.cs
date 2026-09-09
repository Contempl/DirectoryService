using DirectoryService.Application.Messaging;
using DirectoryService.Infrastructure;
using JasperFx.Resources;
using RabbitMqMessaging;
using RabbitMqMessaging.IntegrationEvents.Files;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace DirectoryService.Presentation.Configuration;

public static class RabbitMqConfiguration
{
    public static IHostBuilder AddRabbitMqMessaging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        var rabbitMqOptions = configuration
                                  .GetRequiredSection(RabbitMqOptions.SectionName)
                                  .Get<RabbitMqOptions>()
                              ?? throw new InvalidOperationException(
                                  $"{RabbitMqOptions.SectionName} configuration is missing");

        var databaseConnectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Database connection string is missing");

        // FS-14: Подключаем DirectoryService к RabbitMQ и durable inbox.
        hostBuilder.UseWolverine(options =>
        {
            options.ConfigureRabbitMq(rabbitMqOptions);

            options.PersistMessagesWithPostgresql(
                databaseConnectionString,
                schemaName: ApplicationDbContext.WOLVERINE_SCHEMA);

            options.UseEntityFrameworkCoreTransactions();

            // Consumer находится не в запускаемом Presentation, а в Application.
            options.Discovery.IncludeAssembly(
                typeof(FileDeletedIntegrationEventHandler).Assembly);
            
            options.Policies
                .OnException<InvalidOperationException>()
                .ScheduleRetry(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15))
                .Then
                .MoveToErrorQueue();

            options.ListenToRabbitQueue(
                    "directory-service-file-events",
                    queue =>
                    {
                        queue.BindExchange(
                            FileEventsRouting.EXCHANGE,
                            FileEventsRouting.RoutingKeys.FileDeleted("location"));
                    })
                .UseDurableInbox();
        });

        // FS-14: При старте создаются очередь и служебные таблицы Wolverine.
        hostBuilder.UseResourceSetupOnStartup();

        return hostBuilder;
    }
}