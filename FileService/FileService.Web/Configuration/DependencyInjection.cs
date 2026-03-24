using System.Reflection;
using FileService.Core.Endpoints;
using FileService.Infrastructure;
using FileService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FileService.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FileServiceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

        services.AddEndpoints(typeof(EndpointsExtensions).Assembly);
        
        services.AddS3(configuration);
        
        return services;
    }
}