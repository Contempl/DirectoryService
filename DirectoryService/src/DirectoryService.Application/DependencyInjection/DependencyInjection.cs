using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Application.Departments.Commands.Delete;
using DirectoryService.Application.Departments.Commands.ToggleActivity;
using DirectoryService.Application.Departments.Commands.Update;
using DirectoryService.Application.Departments.Queries.ExpandedDepartments;
using DirectoryService.Application.Departments.Queries.GetChildrenDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentDescendants;
using DirectoryService.Application.Departments.Queries.GetListOfDepartments;
using DirectoryService.Application.Departments.Queries.GetTopDepartments;
using DirectoryService.Application.Locations.BulkActivityUpdate;
using DirectoryService.Application.Locations.AttachPhoto;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Application.Locations.DeletePhoto;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Locations.ReplacePhoto;
using DirectoryService.Application.Locations.Restore;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Locations.UpdateForDepartment;
using DirectoryService.Application.Options;
using DirectoryService.Application.Pagination;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Positions.Delete;
using DirectoryService.Application.Positions.GetById;
using DirectoryService.Application.Positions.Queries;
using DirectoryService.Application.Positions.Update;
using DirectoryService.Application.Search.GlobalSearch;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpdateLocationRequest = DirectoryService.Application.Locations.Update.UpdateLocationRequest;

namespace DirectoryService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheOptions = configuration
            .GetSection(CachingOptions.SectionName)
            .Get<CachingOptions>();
        
        services.AddValidatorsFromAssemblyContaining<CreateDepartmentHandler>();

        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = cacheOptions!.RedisConnectionString;
        });
            
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheOptions!.LocalCacheExpiration ),
                Expiration = TimeSpan.FromMinutes(cacheOptions.Expiration)
            };
        });
        
        services.AddScoped<ICommandHandler<Guid, CreateLocationRequest>, CreateLocationHandler>();
        services.AddScoped<ICommandHandler<Guid, CreatePositionRequest>, CreatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, CreatePositionRequest>,CreatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, DeleteDepartmentRequest>,DeleteDepartmentHandler>();
        services.AddScoped<ICommandHandler<Guid, CreateDepartmentRequest>, CreateDepartmentHandler>();
        services.AddScoped<ICommandHandler<UpdateLocationsRequest>, UpdateLocationsHandler>();
        services.AddScoped<ICommandHandler<Guid, UpdateDepartmentRequest>, UpdateDepartmentHandler>();
        services.AddScoped<ICommandHandler<Guid, DeleteLocationRequest>, DeleteLocationHandler>();
        services.AddScoped<ICommandHandler<Guid, AttachPhotoRequest>, AttachLocationPhotoHandler>();
        services.AddScoped<ICommandHandler<Guid, ReplacePhotoRequest>, ReplaceLocationPhotoHandler>();
        services.AddScoped<ICommandHandler<Guid, DeletePhotoRequest>, DeleteLocationPhotoHandler>();
        services.AddScoped<ICommandHandler<Guid, RestoreLocationRequest>, RestoreLocationHandler>();
        services.AddScoped<ICommandHandler<Location, UpdateLocationRequest> , UpdateLocationHandler>();
        services.AddScoped<ICommandHandler<Position, UpdatePositionCommand> , UpdatePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, DeletePositionRequest> , DeletePositionHandler>();
        services.AddScoped<ICommandHandler<Guid, ToggleDepartmentActivityRequest> , ToggleDepartmentActivityHandler>();
        services.AddScoped<ICommandHandler<BulkUpdateLocationsActivityResult, BulkUpdateLocationsActivityRequest>, BulkUpdateLocationsActivityHandler>();
        services.AddScoped<IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>, GetLocationsHandler>();
        services.AddScoped<IQueryHandler<bool, PagedResult<DepartmentDto>>, GetTopDepartmentsHandler>();
        services.AddScoped<IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>>, GetExpandedDepartmentsHandler>();
        services.AddScoped<IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>>, GetChildrenHandler>();
        services.AddScoped<IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentShortDto>>, GetDepartmentsHandler>();
        services.AddScoped<IQueryHandler<GetDepartmentDescendantIdsQuery, Result<List<Guid>, Errors>>, GetDepartmentDescendantIdsHandler>();
        services.AddScoped<IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>>, GetPositionsHandler>();
        services.AddScoped<IQueryHandler<Guid, Result<PositionDto, Error>>, GetPositionHandler>();
        services.AddScoped<IQueryHandler<SearchQuery, Result<List<SearchResultDto>, Errors>>, GlobalSearchHandler>();
    }
}
