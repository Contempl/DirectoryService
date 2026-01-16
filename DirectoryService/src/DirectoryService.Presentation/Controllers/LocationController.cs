using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateLocationRequest> _createLocationHandler;

    public LocationController(ICommandHandler<Guid, CreateLocationRequest> createLocationHandler)
    {
        _createLocationHandler = createLocationHandler;
    }

    [HttpPost("api/locations")]
    public async Task<EndpointResult<Guid>> CreateLocation(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var request = new CreateLocationRequest(dto);
        return await _createLocationHandler.HandleAsync(request, cancellationToken);
    }
}