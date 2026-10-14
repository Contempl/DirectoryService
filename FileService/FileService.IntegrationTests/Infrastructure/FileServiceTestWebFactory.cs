using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Core.Auth;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FileService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string VideoBucket = "videos";
    public const string PreviewBucket = "preview";
    public const string DocumentsBucket = "documents";

    private const string S3AccessKey = "minioadmin";
    private const string S3SecretKey = "minioadmin";
    private const string RabbitMqUser = "file-service-tests";
    private const string TestIssuer = "http://identity-provider.integration-tests/";
    private const string TestAudience = "directory_api";
    private const string TestSigningKeyId = "file-service-integration-tests";

    private readonly string _rabbitMqPassword = Guid.NewGuid().ToString("N");
    private readonly RSA _testSigningKey = RSA.Create(2048);

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

    private readonly IContainer _rabbitMqContainer;

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
    private IAmazonS3 _s3Client = null!;

    public FileServiceTestWebFactory()
    {
        _rabbitMqContainer = new ContainerBuilder("rabbitmq:management")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", _rabbitMqPassword)
            .WithPortBinding(5672, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5672))
            .Build();
    }

    public string S3Endpoint => $"http://{_s3Container.Hostname}:{_s3Container.GetMappedPublicPort(9000)}";

    public string CreateTestAccessToken(string role = SystemRoles.Admin)
    {
        var signingKey = new RsaSecurityKey(_testSigningKey)
        {
            KeyId = TestSigningKeyId
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", "tests@example.local"),
                new Claim("name", "Integration Tests"),
                new Claim("role", role)
            ],
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.RsaSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _s3Container.StartAsync();
        await _rabbitMqContainer.StartAsync();

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
        _s3Client?.Dispose();
        if (_dbConnection is not null)
            await _dbConnection.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await _s3Container.DisposeAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
        _testSigningKey.Dispose();
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

        // FS-14: Wolverine читает connection string во время построения host-а,
        // поэтому передаём Testcontainers-БД как host setting до запуска Program.cs.
        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        builder.UseSetting("RabbitMq:Host", _rabbitMqContainer.Hostname);
        builder.UseSetting("RabbitMq:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMq:UserName", RabbitMqUser);
        builder.UseSetting("RabbitMq:Password", _rabbitMqPassword);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString(),
                ["RabbitMq:Host"] = _rabbitMqContainer.Hostname,
                ["RabbitMq:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                ["RabbitMq:UserName"] = RabbitMqUser,
                ["RabbitMq:Password"] = _rabbitMqPassword,
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
                ["IdentityProvider:Authority"] = TestIssuer,
                ["IdentityProvider:MetadataAddress"] = "http://identity-provider.integration-tests/.well-known/openid-configuration",
                ["IdentityProvider:Audience"] = TestAudience,
                ["IdentityProvider:RequireHttpsMetadata"] = "false",
                ["DevAuth:Enabled"] = "false",
                ["CacheOptions:RedisConnectionString"] = "unused-in-integration-tests",
                ["CacheOptions:ExpirationTimeInMinutes"] = "35",
                ["CacheOptions:LocalCacheExpiration"] = "5"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();

            var publicKey = new RsaSecurityKey(_testSigningKey.ExportParameters(false))
            {
                KeyId = TestSigningKeyId
            };
            var providerConfiguration = new OpenIdConnectConfiguration
            {
                Issuer = TestIssuer
            };
            providerConfiguration.SigningKeys.Add(publicKey);

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options => options.ConfigurationManager =
                    new StaticConfigurationManager<OpenIdConnectConfiguration>(
                        providerConfiguration));
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
