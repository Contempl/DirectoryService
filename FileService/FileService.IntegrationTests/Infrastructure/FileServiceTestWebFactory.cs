using System.Data.Common;
using Amazon.S3;
using Amazon.S3.Model;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FileService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests;

public class FileServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string VideoBucket = "videos";
    public const string PreviewBucket = "preview";
    public const string DocumentsBucket = "documents";

    private const string S3AccessKey = "minioadmin";
    private const string S3SecretKey = "minioadmin";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("file_service_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly IContainer _s3Container = new ContainerBuilder()
        .WithImage("minio/minio:latest")
        .WithCommand("server", "/data")
        .WithEnvironment("MINIO_ROOT_USER", S3AccessKey)
        .WithEnvironment("MINIO_ROOT_PASSWORD", S3SecretKey)
        .WithPortBinding(9000, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(9000).ForPath("/minio/health/ready")))
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
    private IAmazonS3 _s3Client = null!;

    public FileServiceTestWebFactory()
    {
        Environment.SetEnvironmentVariable("JwtOptions__Issuer", "file-service-integration-tests");
        Environment.SetEnvironmentVariable("JwtOptions__Audience", "file-service-integration-tests");
        Environment.SetEnvironmentVariable("JwtOptions__Secret", "superlongsecrettobeatleast32characters");
        Environment.SetEnvironmentVariable("JwtOptions__AccessTokenLifetimeMinutes", "15");
        Environment.SetEnvironmentVariable("JwtOptions__RefreshTokenLifetime", "7");
    }

    public string S3Endpoint => $"http://{_s3Container.Hostname}:{_s3Container.GetMappedPublicPort(9000)}";

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _s3Container.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });

        _s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await EnsureBucketExists(VideoBucket);
        await EnsureBucketExists(PreviewBucket);
        await EnsureBucketExists(DocumentsBucket);
    }

    public new async Task DisposeAsync()
    {
        _s3Client.Dispose();
        await _dbConnection.DisposeAsync();
        await _s3Container.DisposeAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetStateAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        await ClearBucketAsync(VideoBucket);
        await ClearBucketAsync(PreviewBucket);
        await ClearBucketAsync(DocumentsBucket);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString(),
                ["S3Options:Endpoint"] = S3Endpoint,
                ["S3Options:AccessKey"] = S3AccessKey,
                ["S3Options:SecretKey"] = S3SecretKey,
                ["S3Options:WithSsl"] = "false",
                ["S3Options:DownloadUrlExpirationHours"] = "24",
                ["S3Options:UploadUrlExpirationMinutes"] = "5",
                ["S3Options:UploadUrlExpirationHours"] = "1",
                ["S3Options:RecommendedChunkSizeBytes"] = "5242880",
                ["S3Options:MaxChunks"] = "100",
                ["S3Options:MaxConcurrentRequests"] = "10",
                ["S3Options:RequiredBuckets:0"] = VideoBucket,
                ["S3Options:RequiredBuckets:1"] = PreviewBucket,
                ["S3Options:RequiredBuckets:2"] = DocumentsBucket,
                ["JwtOptions:Issuer"] = "file-service-integration-tests",
                ["JwtOptions:Audience"] = "file-service-integration-tests",
                ["JwtOptions:Secret"] = "superlongsecrettobeatleast32characters",
                ["JwtOptions:AccessTokenLifetimeMinutes"] = "15",
                ["JwtOptions:RefreshTokenLifetime"] = "7",
                ["DevAuth:Enabled"] = "true"
            });
        });
    }

    private async Task EnsureBucketExists(string bucketName)
    {
        if (await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName))
            return;

        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
    }

    private async Task ClearBucketAsync(string bucketName)
    {
        var listResponse = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = bucketName });
        if (listResponse.S3Objects is null || listResponse.S3Objects.Count == 0)
            return;

        await _s3Client.DeleteObjectsAsync(new DeleteObjectsRequest
        {
            BucketName = bucketName,
            Objects = listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
        });
    }
}
