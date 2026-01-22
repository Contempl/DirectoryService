using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateLocationRequest> _createLocationHandler;
    private readonly IQueryHandler<GetLocationsRequest, GetLocationsDto?> _getLocationsHandler;

    public LocationController(
        ICommandHandler<Guid, CreateLocationRequest> createLocationHandler,
        IQueryHandler<GetLocationsRequest, GetLocationsDto?> getLocationsHandler)
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
    public async Task<ActionResult<GetLocationsDto>> GetLocations([FromQuery]GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var result =  await _getLocationsHandler.HandleAsync(request, cancellationToken);
        
        return Ok(result);
    }
}