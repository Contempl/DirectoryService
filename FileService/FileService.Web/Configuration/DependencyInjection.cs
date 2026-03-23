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
        
        return services;
    }
}