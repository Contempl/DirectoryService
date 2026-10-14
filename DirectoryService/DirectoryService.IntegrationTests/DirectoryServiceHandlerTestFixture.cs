using System.Data.Common;
using DirectoryService.Application.DependencyInjection;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.DI;
using FileService.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests;

public sealed class DirectoryServiceHandlerTestFixture : IAsyncLifetime
{
    public FakeFileCommunicationService FileService { get; } = new();

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("directory_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private ServiceProvider? _services;
    private DbConnection? _connection;
    private Respawner? _respawner;

    public IServiceProvider Services => _services
        ?? throw new InvalidOperationException("Test fixture is not initialized.");

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        var connectionString = _database.GetConnectionString();

        await using (var setupConnection = new NpgsqlConnection(connectionString))
        {
            await setupConnection.OpenAsync();
            await using var command = new NpgsqlCommand(
                "CREATE EXTENSION IF NOT EXISTS ltree;", setupConnection);
            await command.ExecuteNonQueryAsync();
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = connectionString,
                ["CachingOptions:RedisEnabled"] = "false",
                ["CachingOptions:LocalCacheExpiration"] = "5",
                ["CachingOptions:Expiration"] = "5",
                ["AuthServiceOptions:BaseAddress"] = "http://auth-service.test",
                ["AuthServiceOptions:TimeoutSeconds"] = "10",
                ["ServiceTokenOptions:ClientId"] = "directory-service-tests",
                ["ServiceTokenOptions:ClientSecret"] = "integration-test-only-secret",
                ["ServiceTokenOptions:Scope"] = "directory_api",
                ["AuthUsersSynchronizationOptions:Enabled"] = "false",
                ["FileServiceOptions:Url"] = "http://file-service.test"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);
        services.RemoveAll<IFileCommunicationService>();
        services.AddSingleton<IFileCommunicationService>(FileService);
        services.AddDistributedMemoryCache();
        _services = services.BuildServiceProvider(validateScopes: true);

        await using (var scope = _services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            await connection.ReloadTypesAsync();
            await connection.CloseAsync();
        }

        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _connection is null)
            throw new InvalidOperationException("Test fixture is not initialized.");

        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        if (_services is not null)
            await _services.DisposeAsync();
        if (_connection is not null)
            await _connection.DisposeAsync();
        await _database.DisposeAsync();
    }
}
