using JasperFx.Resources;
using RabbitMqMessaging;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace FileService.Configuration;

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
                                  $"Configuration section '{RabbitMqOptions.SectionName}' is invalid.");
        
        var databaseConnectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' is required.");

        hostBuilder.UseWolverine(options =>
        {
            options.ConfigureRabbitMq(rabbitMqOptions);
            
            // FS-14: PostgreSQL хранит durable inbox/outbox сообщения.
            options.PersistMessagesWithPostgresql(
                databaseConnectionString,
                schemaName: Infrastructure.Postgres.FileServiceDbContext.WOLVERINE_SCHEMA);

            // FS-14: Связываем сообщения с EF Core транзакциями.
            options.UseEntityFrameworkCoreTransactions();
        });

        hostBuilder.UseResourceSetupOnStartup();
        
        return hostBuilder;
    }
}