using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreatePositionRequest> _positionHandler;

    public PositionController(ICommandHandler<Guid, CreatePositionRequest> positionService)
    {
        _positionHandler = positionService;
    }


    [HttpPost("/api/positions")]
    public async Task<EndpointResult<Guid>> CreatePosition(CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        return await _positionHandler.HandleAsync(request, cancellationToken);
    }
}