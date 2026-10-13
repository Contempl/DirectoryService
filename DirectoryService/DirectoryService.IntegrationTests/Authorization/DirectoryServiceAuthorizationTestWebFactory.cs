using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using DirectoryService.Infrastructure;
using DirectoryService.Presentation;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Authorization;

public sealed class DirectoryServiceAuthorizationTestWebFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestIssuer = "http://identity-provider.integration-tests/";
    private const string TestAudience = "directory_api";
    private const string TestSigningKeyId = "directory-service-integration-tests";
    private const string RabbitMqUser = "directory-service-tests";

    private readonly HttpMessageHandler? _identityProviderHandler;
    private readonly string _issuer;
    private readonly string _rabbitMqPassword = Guid.NewGuid().ToString("N");
    private readonly RSA _testSigningKey = RSA.Create(2048);
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("directory_service_auth_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly IContainer _rabbitMq;

    public DirectoryServiceAuthorizationTestWebFactory()
        : this(null)
    {
    }

    public static DirectoryServiceAuthorizationTestWebFactory WithIdentityProvider(
        HttpMessageHandler identityProviderHandler) =>
        new(identityProviderHandler);

    private DirectoryServiceAuthorizationTestWebFactory(
        HttpMessageHandler? identityProviderHandler)
    {
        _identityProviderHandler = identityProviderHandler;
        _issuer = identityProviderHandler is null
            ? TestIssuer
            : "http://localhost/";
        _rabbitMq = new ContainerBuilder("rabbitmq:management")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", _rabbitMqPassword)
            .WithPortBinding(5672, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(5672))
            .Build();
    }

    public string CreateTestAccessToken(
        string role,
        bool foreignSignature = false,
        string? signingKeyId = null)
    {
        using var foreignKey = foreignSignature ? RSA.Create(2048) : null;
        var signingKey = new RsaSecurityKey(foreignKey ?? _testSigningKey)
        {
            KeyId = signingKeyId ?? TestSigningKeyId
        };
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: TestAudience,
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", "user@integration.test"),
                new Claim("name", "Integration Test"),
                new Claim("role", role)
            ],
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.RsaSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await _rabbitMq.StartAsync();
        _ = Services;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _database.DisposeAsync();
        _testSigningKey.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Database", _database.GetConnectionString());
        builder.UseSetting("RabbitMq:Host", _rabbitMq.Hostname);
        builder.UseSetting("RabbitMq:Port", _rabbitMq.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMq:UserName", RabbitMqUser);
        builder.UseSetting("RabbitMq:Password", _rabbitMqPassword);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _database.GetConnectionString(),
                ["RabbitMq:Host"] = _rabbitMq.Hostname,
                ["RabbitMq:Port"] = _rabbitMq.GetMappedPublicPort(5672).ToString(),
                ["RabbitMq:UserName"] = RabbitMqUser,
                ["RabbitMq:Password"] = _rabbitMqPassword,
                ["IdentityProvider:Authority"] = _issuer,
                ["IdentityProvider:MetadataAddress"] =
                    $"{_issuer.TrimEnd('/')}/.well-known/openid-configuration",
                ["IdentityProvider:Audience"] = TestAudience,
                ["IdentityProvider:RequireHttpsMetadata"] = "false",
                ["DevAuth:Enabled"] = "false",
                ["ServiceTokenOptions:ClientSecret"] = "integration-test-only-secret",
                ["CachingOptions:RedisEnabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IDistributedCache>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_database.GetConnectionString()));
            services.AddDistributedMemoryCache();

            if (_identityProviderHandler is not null)
            {
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        var retriever = new HttpDocumentRetriever(
                            new HttpClient(
                                _identityProviderHandler,
                                disposeHandler: false))
                        {
                            RequireHttps = false
                        };
                        options.ConfigurationManager =
                            new ConfigurationManager<OpenIdConnectConfiguration>(
                                $"{_issuer.TrimEnd('/')}/.well-known/openid-configuration",
                                new OpenIdConnectConfigurationRetriever(),
                                retriever);
                    });
                return;
            }

            var providerConfiguration = new OpenIdConnectConfiguration
            {
                Issuer = TestIssuer
            };
            providerConfiguration.SigningKeys.Add(new RsaSecurityKey(
                _testSigningKey.ExportParameters(false))
            {
                KeyId = TestSigningKeyId
            });

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options => options.ConfigurationManager =
                    new StaticConfigurationManager<OpenIdConnectConfiguration>(
                        providerConfiguration));
        });
    }
}
