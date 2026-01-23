using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateLocationRequest> _createLocationHandler;
    private readonly IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> _getLocationsHandler;

    public LocationController(
        ICommandHandler<Guid, CreateLocationRequest> createLocationHandler,
        IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>> getLocationsHandler)
    {
        _createLocationHandler = createLocationHandler;
        _getLocationsHandler = getLocationsHandler;
    }

    [HttpPost("api/locations")]
    public async Task<EndpointResult<Guid>> CreateLocation(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var request = new CreateLocationRequest(dto);
        return await _createLocationHandler.HandleAsync(request, cancellationToken);
    }

    [HttpGet("api/locations")]
    public async Task<ActionResult<PagedResult<LocationDto>>> GetLocations(
        [FromQuery]GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var result =  await _getLocationsHandler.HandleAsync(query, cancellationToken);
        
        return Ok(result);
    }
}