using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Application.Departments.Commands.Update;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Departments.Queries.ChildrenDepartments;
using DirectoryService.Application.Departments.Queries.ExpandedDepartments;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Pagination;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Departments;
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
        services.AddScoped<ICommandHandler<Guid, CreatePositionRequest>, CreatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, CreatePositionRequest>,CreatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, CreateDepartmentRequest>, CreateDepartmentHandler>();
        services.AddScoped<ICommandHandler<UpdateLocationRequest>, UpdateLocationHandler>();
        services.AddScoped<ICommandHandler<Guid, UpdateDepartmentRequest>, UpdateDepartmentHandler>();
        services.AddScoped<IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>, GetLocationsHandler>();
        services.AddScoped<IQueryHandler<bool, PagedResult<DepartmentDto>>, GetTopDepartmentsHandler>();
        services.AddScoped<IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>>, GetExpandedDepartmentsHandler>();
        services.AddScoped<IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>>, GetChildrenHandler>();
    }
}