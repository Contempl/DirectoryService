using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsHandler : IQueryHandler<GetLocationsQuery, GetLocationsDto?>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationsHandler(IReadDbContext context)
    {
        _readDbContext = context;
    }

    public async Task<GetLocationsDto?> HandleAsync(GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var locationsQuery = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(query.Search))
            locationsQuery = locationsQuery.Where(l =>
                EF.Functions.Like(l.Name.Value.ToLower(), $"%{query.Search.ToLower()}%"));

        locationsQuery = locationsQuery.Where(l => l.IsActive == query.IsActive);

        if (query.DepartmentIds is { Length: > 0 })
        {
            locationsQuery = locationsQuery.Where(loc => 
                loc.DepartmentLocations.Any(dl => query.DepartmentIds.Contains(dl.DepartmentId)));
        }

        var totalCount = await locationsQuery.LongCountAsync(cancellationToken);

        locationsQuery = locationsQuery
            .OrderBy(l => l.UpdatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var locations = await locationsQuery
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name.Value,
                Timezone = l.Timezone.Value,
                Address = new AddressDto(l.Address.City, l.Address.Street, l.Address.House, l.Address.Apartment),
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetLocationsDto(locations, totalCount);
    }
}