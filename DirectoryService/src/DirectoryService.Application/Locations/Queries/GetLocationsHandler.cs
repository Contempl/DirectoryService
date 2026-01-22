using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsHandler : IQueryHandler<GetLocationsRequest, GetLocationsDto?>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationsHandler(IReadDbContext context)
    {
        _readDbContext = context;
    }

    public async Task<GetLocationsDto?> HandleAsync(GetLocationsRequest request, CancellationToken cancellationToken)
    {
        var locationsQuery = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(request.Search))
            locationsQuery = locationsQuery.Where(l =>
                EF.Functions.Like(l.Name.Value.ToLower(), $"%{request.Search.ToLower()}%"));

        locationsQuery = locationsQuery.Where(l => l.IsActive == request.IsActive);

        if (request.DepartmentIds is { Length: > 0 })
        {
            locationsQuery = locationsQuery.Where(loc => 
                loc.DepartmentLocations.Any(dl => request.DepartmentIds.Contains(dl.DepartmentId)));
        }

        var totalCount = await locationsQuery.LongCountAsync(cancellationToken);

        locationsQuery = locationsQuery
            .OrderBy(l => l.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var locations = await locationsQuery
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Timezone = l.Timezone,
                Address = l.Address,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetLocationsDto(locations, totalCount);
    }
}