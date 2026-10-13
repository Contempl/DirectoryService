using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace AuthService.IntegrationTests;

public sealed class AuthServiceTestWebFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string FrontendClientId = "directory-web-tests";
    public const string FrontendClientSecret =
        "integration-test-web-client-secret";
    public const string ServiceClientId = "directory-service-tests";
    public const string ServiceClientSecret =
        "integration-test-service-client-secret";
    public const string RedirectUri = "http://client.test/callback";
    public const string ApiScope = "directory_api";
    public const string AdminEmail = "admin@integration.test";
    public const string AdminPassword = "IntegrationAdmin123!";

    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder("postgres:17")
            .WithDatabase("auth_service_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        _ = Services;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting(
            "ConnectionStrings:Database",
            _database.GetConnectionString());

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] =
                        _database.GetConnectionString(),
                    ["AuthorizationServer:Issuer"] = "http://localhost",
                    ["AuthorizationServer:LoginPageUrl"] =
                        "http://client.test/login",
                    ["AuthorizationServer:ApiScope"] = ApiScope,
                    ["OpenIddictClients:Frontend:ClientId"] =
                        FrontendClientId,
                    ["OpenIddictClients:Frontend:ClientSecret"] =
                        FrontendClientSecret,
                    ["OpenIddictClients:Frontend:DisplayName"] =
                        "Integration test web client",
                    ["OpenIddictClients:Frontend:RedirectUri"] =
                        RedirectUri,
                    ["OpenIddictClients:DirectoryService:ClientId"] =
                        ServiceClientId,
                    ["OpenIddictClients:DirectoryService:ClientSecret"] =
                        ServiceClientSecret,
                    ["OpenIddictClients:DirectoryService:DisplayName"] =
                        "Integration test service client",
                    ["AdminOptions:Username"] = "integration-admin",
                    ["AdminOptions:FirstName"] = "Integration",
                    ["AdminOptions:LastName"] = "Admin",
                    ["AdminOptions:Email"] = AdminEmail,
                    ["AdminOptions:Password"] = AdminPassword
                });
        });
    }
}
