using System.Data.Common;
using Amazon.S3;
using FileService.Core;
using FileService.Infrastructure;
using FileService.Infrastructure.Postgres;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests;

public class FileServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_dbConnection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            services.AddDbContext<FileServiceDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            services.RemoveAll<IS3Provider>();
            services.AddScoped<IS3Provider, FakeS3Provider>();

            services.RemoveAll<IAmazonS3>();

            var s3Init = services.FirstOrDefault(
                d => d.ImplementationType == typeof(S3BucketInitializationService));
            if (s3Init is not null)
                services.Remove(s3Init);
        });
    }

    private async Task InitializeRespawner() =>
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["public"] });
}
