using DirectoryService.Application.Positions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class PositionController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionController(IPositionService positionService)
    {
        _positionService = positionService;
    }


    [HttpPost("/api/positions")]
    public async Task<EndpointResult<Guid>> CreatePosition(CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        return await _positionService.HandleAsync(request, cancellationToken);
    }
}