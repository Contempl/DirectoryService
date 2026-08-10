using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Locations;
using FileService.Contracts;
using FileService.Contracts.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsHandler : IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ILogger<GetLocationsHandler> _logger;

    public GetLocationsHandler(
        IReadDbContext context,
        IFileCommunicationService fileCommunicationService,
        ILogger<GetLocationsHandler> logger)
    {
        _readDbContext = context;
        _fileCommunicationService = fileCommunicationService;
        _logger = logger;
    }

    public async Task<PagedResult<LocationDto>> HandleAsync(GetLocationsQuery query, CancellationToken cancellationToken)
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
                UpdatedAt = l.UpdatedAt,
                Photo = l.Photo == null
                    ? null
                    : new LocationPhotoDto
                    {
                        AssetId = l.Photo.AssetId,
                        Status = LocationPhotoStatuses.TemporarilyUnavailable,
                        FileName = l.Photo.FileName,
                        ContentType = l.Photo.ContentType,
                        Size = l.Photo.Size,
                        VerifiedAt = l.Photo.VerifiedAt,
                        ContentUrl = null
                    }
            })
            .ToListAsync(cancellationToken);

        await EnrichPhotos(locations, cancellationToken);

        return new PagedResult<LocationDto>
        {
            Items = locations, 
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
            TotalCount = totalCount
        };
    }

    private async Task EnrichPhotos(List<LocationDto> locations, CancellationToken cancellationToken)
    {
        var assetIds = locations
            .Where(location => location.Photo is not null)
            .Select(location => location.Photo!.AssetId)
            .Distinct()
            .ToList();

        if (assetIds.Count == 0)
            return;

        var assetsResult = await _fileCommunicationService.GetMediaAssets(
            new GetMediaAssetsInfoRequest(assetIds),
            cancellationToken);

        if (assetsResult.IsFailure)
        {
            _logger.LogWarning(
                "Could not enrich location photos from File Service: {ErrorCode}",
                assetsResult.Error.Code);
            return;
        }

        var assets = assetsResult.Value.MediaAssets.ToDictionary(asset => asset.Id);

        for (var index = 0; index < locations.Count; index++)
        {
            var location = locations[index];
            if (location.Photo is null)
                continue;

            if (!assets.TryGetValue(location.Photo.AssetId, out var asset))
            {
                locations[index] = location with
                {
                    Photo = location.Photo with
                    {
                        Status = LocationPhotoStatuses.Missing,
                        ContentUrl = null
                    }
                };
                continue;
            }

            locations[index] = location with
            {
                Photo = location.Photo with
                {
                    Status = MapStatus(asset),
                    ContentUrl = IsReady(asset) ? asset.DownloadUrl : null
                }
            };
        }
    }

    private static bool IsReady(MediaAssetBriefDto asset) =>
        string.Equals(asset.Status, "ready", StringComparison.OrdinalIgnoreCase);

    private static string MapStatus(MediaAssetBriefDto asset)
    {
        if (string.Equals(asset.Status, "deleted", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoStatuses.Deleted;

        return IsReady(asset)
            ? LocationPhotoStatuses.Available
            : LocationPhotoStatuses.TemporarilyUnavailable;
    }
}
