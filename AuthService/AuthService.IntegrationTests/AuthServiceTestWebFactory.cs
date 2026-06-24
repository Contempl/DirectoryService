using System.Data.Common;
using AuthService.Application;
using AuthService.Core;
using AuthService.Core.Identity;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace AuthService.IntegrationTests;

public class AuthServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres")
        .WithDatabase("auth_service_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public const string TestAdminEmail = "admin@test.com";
    public const string TestAdminPassword = "Admin@Test123!";

    public FakeEmailSender FakeEmailSender { get; } = new();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Accessing Services triggers app build which runs UseMigrations()
        await using var scope = Services.CreateAsyncScope();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore =
                [
                    new Table("AspNetRoles"),
                    new Table("AspNetRoleClaims")
                ]
            });
    }

    public new async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_dbConnection);

    public async Task CreateAdminAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existing = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (existing.Any()) return;

        var adminResult = ApplicationUser.SeedAdmin("Admin", "Test", TestAdminEmail, "admin_test");
        var admin = adminResult.Value;

        await userManager.CreateAsync(admin, TestAdminPassword);
        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminOptions:Email"] = TestAdminEmail,
                ["AdminOptions:Password"] = TestAdminPassword,
                ["AdminOptions:FirstName"] = "Admin",
                ["AdminOptions:LastName"] = "Test",
                ["AdminOptions:Username"] = "admin_test",
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AuthDbContext>();
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender>(_ => FakeEmailSender);

            var deleteService = services.FirstOrDefault(
                d => d.ImplementationType == typeof(DeleteRevokedTokensService));
            if (deleteService is not null)
                services.Remove(deleteService);
        });
    }
}
