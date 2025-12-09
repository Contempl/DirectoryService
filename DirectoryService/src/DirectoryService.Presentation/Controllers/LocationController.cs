using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpPost("api/locations")]
    public async Task<IActionResult> CreateLocation(CreateLocationDto request, CancellationToken cancellationToken)
    {
        var result = await _locationService.CreateLocationAsync(request, cancellationToken);
        return Ok(result);
    }
}