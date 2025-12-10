using DirectoryService.Application.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILocationService, LocationService>();
    }
}