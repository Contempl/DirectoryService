using System.Security.Claims;
using AuthService.Core.Options;
using AuthService.Domain.Entities;
using Core.Auth;
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
        
        foreach (var roleName in SystemRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new IdentityRole<Guid>(roleName) { Id = Guid.CreateVersion7() };
            var createRoleResult = await roleManager.CreateAsync(role);

            if (!createRoleResult.Succeeded)
            {
                var errors = string.Join(", ", createRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to create role {roleName}: {errors}");
            }

            _logger.LogInformation("Created role: {Role}", roleName);
        }
    }
    
    private async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var existingAdmins = await userManager.GetUsersInRoleAsync(SystemRoles.Admin);
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

        var addRoleResult = await userManager.AddToRoleAsync(admin, SystemRoles.Admin);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addRoleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign Admin role: {errors}");
        }

        _logger.LogInformation("Admin user created: {Email}", admin.Email);
    }
    
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
