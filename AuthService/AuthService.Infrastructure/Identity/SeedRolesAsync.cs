using System.Security.Claims;
using AuthService.Core.Options;
using AuthService.Domain.Authorization;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Core.Identity;

public class SeedDataService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AdminOptions _adminOptions;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(
        IServiceProvider serviceProvider,
        IOptions<AdminOptions> adminOptions,
        ILogger<SeedDataService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _adminOptions = adminOptions.Value;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
    }

    public async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {

        if (string.IsNullOrEmpty(_adminOptions.Email) || string.IsNullOrEmpty(_adminOptions.Password))
        {
            _logger.LogError("Email or password is missing from config");
            throw new ArgumentException("Email and password are required.");
        }    
        
        _logger.LogInformation("Started seeding roles...");
        
        foreach (var roleName in Roles.AllRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new IdentityRole<Guid>(roleName);
            await roleManager.CreateAsync(role);

            var permissions = RolePermissions.GetPermissions([roleName]);
            foreach (var permission in permissions)
            {
                await roleManager.AddClaimAsync(role, new Claim("permission", permission));
            }
            _logger.LogInformation("Created role: {Role}", roleName);
        }
    }
    
    private async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var existingAdmins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (existingAdmins.Any())
        {
            _logger.LogInformation("Admin user already exists, skipping.");
            return;
        }

        _logger.LogInformation("Creating default admin user...");
        
        var adminResult = ApplicationUser.SeedAdmin(
            _adminOptions.FirstName,
            _adminOptions.LastName, 
            _adminOptions.Email,
            _adminOptions.Username);

        if (adminResult.IsFailure)
        {
            _logger.LogError("Failed to create admin user: {Error}", adminResult.Error);
            return;
        }
        
        var admin = adminResult.Value;

        var result = await userManager.CreateAsync(admin, _adminOptions.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create admin: {errors}");
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
        _logger.LogInformation("Admin user created: {Email}", admin.Email);
    }
    
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}