using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.DI;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocationRepository, LocationRepository>();
    }
}