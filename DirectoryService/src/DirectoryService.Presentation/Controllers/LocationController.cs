using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Application.Locations.Queries;
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
    private readonly IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> _getLocationsHandler;

    public LocationController(
        ICommandHandler<Guid, CreateLocationRequest> createLocationHandler,
        IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> getLocationsHandler,
        ICommandHandler<Location, UpdateLocationRequest> updateLocationHandler, 
        ICommandHandler<Guid, DeleteLocationRequest> deleteLocationHandler,
        ICommandHandler<Guid, RestoreLocationRequest> restoreLocationHandler)
    {
        _createLocationHandler = createLocationHandler;
        _getLocationsHandler = getLocationsHandler;
        _updateLocationHandler = updateLocationHandler;
        _deleteLocationHandler = deleteLocationHandler;
        _restoreLocationHandler = restoreLocationHandler;
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
        [FromQuery]GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var result =  await _getLocationsHandler.HandleAsync(query, cancellationToken);
        
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
        var request = new  DeleteLocationRequest(locationId);
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
}
