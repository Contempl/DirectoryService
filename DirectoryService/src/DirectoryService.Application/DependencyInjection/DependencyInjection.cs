using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateLocationRequest>, CreateLocationValidator>();
        services.AddScoped<ICommandHandler<Guid, CreateLocationRequest>, CreateLocationHandler>();
    }
}