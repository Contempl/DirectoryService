using AuthService.Application.Features.RefreshToken;
using AuthService.Application.Features.Register;
using AuthService.Core;
using AuthService.Core.Identity;
using AuthService.Core.Options;
using AuthService.Domain.Entities;
using Core.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;

namespace AuthService.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.AddHostedService<DeleteRevokedTokensService>();
        
        services.AddScoped<ICommandHandler<RefreshToken, RefreshTokenRequest>, RefreshTokenHandler>();
        services.AddScoped<ICommandHandler<Guid, RegisterRequest>, RegisterHandler>();

        return services;
    }

    public static IServiceCollection AddDatabaseWithLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

        services.Configure<AdminOptions>(
            configuration.GetSection(nameof(AdminOptions))
        );

        services.Configure<JwtOptions>(
            configuration.GetSection(nameof(JwtOptions))
        );

        services.Configure<BackgroundServiceOptions>(
            configuration.GetSection(nameof(BackgroundServiceOptions))
        );
        
        return services;
    }
}