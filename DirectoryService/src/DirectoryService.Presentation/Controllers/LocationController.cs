using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.AttachPhoto;
using DirectoryService.Application.Locations.BulkActivityUpdate;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Application.Locations.DeletePhoto;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Locations.ReplacePhoto;
using DirectoryService.Application.Locations.Restore;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Presentation.Response;
using Framework.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateLocationRequest> _createLocationHandler;
    private readonly ICommandHandler<Location, UpdateLocationRequest> _updateLocationHandler;
    private readonly ICommandHandler<Guid, DeleteLocationRequest> _deleteLocationHandler;
    private readonly ICommandHandler<Guid, RestoreLocationRequest> _restoreLocationHandler;
    private readonly ICommandHandler<Guid, ReplacePhotoRequest> _replacePhotoHandler;
    private readonly ICommandHandler<Guid, DeletePhotoRequest> _removePhotoHandler;
    private readonly ICommandHandler<Guid, AttachPhotoRequest> _attachPhotoHandler;

    private readonly ICommandHandler<BulkUpdateLocationsActivityResult, BulkUpdateLocationsActivityRequest>
        _bulkUpdateActivityHandler;

    private readonly IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> _getLocationsHandler;

    public LocationController(
        ICommandHandler<Guid, CreateLocationRequest> createLocationHandler,
        IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> getLocationsHandler,
        ICommandHandler<Location, UpdateLocationRequest> updateLocationHandler,
        ICommandHandler<Guid, DeleteLocationRequest> deleteLocationHandler,
        ICommandHandler<Guid, RestoreLocationRequest> restoreLocationHandler,
        ICommandHandler<BulkUpdateLocationsActivityResult, BulkUpdateLocationsActivityRequest>
            bulkUpdateActivityHandler,
        ICommandHandler<Guid, ReplacePhotoRequest> replacePhotoHandler,
        ICommandHandler<Guid, DeletePhotoRequest> removePhotoHandler,
        ICommandHandler<Guid, AttachPhotoRequest> attachPhotoHandler)
    {
        _createLocationHandler = createLocationHandler;
        _getLocationsHandler = getLocationsHandler;
        _updateLocationHandler = updateLocationHandler;
        _deleteLocationHandler = deleteLocationHandler;
        _restoreLocationHandler = restoreLocationHandler;
        _bulkUpdateActivityHandler = bulkUpdateActivityHandler;
        _replacePhotoHandler = replacePhotoHandler;
        _removePhotoHandler = removePhotoHandler;
        _attachPhotoHandler = attachPhotoHandler;
    }

    [HttpPost("api/locations")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> CreateLocation(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var request = new CreateLocationRequest(dto);
        return await _createLocationHandler.HandleAsync(request, cancellationToken);
    }

    [HttpGet("api/locations")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<PagedResult<LocationDto>>> GetLocations(
        [FromQuery] GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getLocationsHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpPut("api/locations/{locationId}")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Location>> UpdateLocation(
        [FromRoute] Guid locationId,
        [FromBody] UpdateLocationDto updateLocationDto,
        CancellationToken cancellationToken)
    {
        var request = new UpdateLocationRequest(locationId, updateLocationDto);
        return await _updateLocationHandler.HandleAsync(request, cancellationToken);
    }

    [HttpDelete("api/locations/{locationId}")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> DeleteLocation(
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var request = new DeleteLocationRequest(locationId);
        return await _deleteLocationHandler.HandleAsync(request, cancellationToken);
    }

    [HttpPut("api/locations/{locationId}/restore")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> RestoreLocation(
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        return await _restoreLocationHandler.HandleAsync(
            new RestoreLocationRequest(locationId),
            cancellationToken);
    }

    [HttpPut("api/locations/bulk/activity")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<BulkUpdateLocationsActivityResult>>
        BulkUpdateActivity(
            BulkUpdateLocationsActivityRequest request,
            CancellationToken cancellationToken)
    {
        return await _bulkUpdateActivityHandler.HandleAsync(request, cancellationToken);
    }

    [HttpPost("api/locations/{locationId}/photo")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> AttachLocationPhoto(
        [FromRoute] Guid locationId,
        [FromBody] SetLocationPhotoDto dto,
        CancellationToken cancellationToken)
    {
        var request = new AttachPhotoRequest(locationId, dto.AssetId);

        return await _attachPhotoHandler.HandleAsync(request, cancellationToken);
        
    }

    [HttpPut("api/locations/{locationId}/photo")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> ReplaceLocationPhoto(
        [FromRoute] Guid locationId,
        [FromBody] SetLocationPhotoDto dto,
        CancellationToken cancellationToken)
    {
        var request = new ReplacePhotoRequest(locationId, dto.AssetId);

        return await _replacePhotoHandler.HandleAsync(request, cancellationToken);
    }

    [HttpDelete("api/locations/{locationId}/photo")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> DeleteLocationPhoto(
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var request = new DeletePhotoRequest(locationId);

        return await _removePhotoHandler.HandleAsync(request, cancellationToken);
    }
}