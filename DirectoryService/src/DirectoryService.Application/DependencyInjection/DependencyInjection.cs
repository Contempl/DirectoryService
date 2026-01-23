using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Update;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateDepartmentHandler>();
        services.AddScoped<ICommandHandler<Guid, CreateLocationRequest>, CreateLocationHandler>();
        services.AddScoped<ICommandHandler<Guid, CreatePositionRequest>,CreatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, CreateDepartmentRequest>, CreateDepartmentHandler>();
        services.AddScoped<ICommandHandler<UpdateLocationRequest>, UpdateLocationHandler>();
        services.AddScoped<ICommandHandler<Guid, UpdateDepartmentRequest>, UpdateDepartmentHandler>();
        services.AddScoped<IQueryHandler<GetLocationsQuery, GetLocationsDto?>, GetLocationsHandler>();
    }
}