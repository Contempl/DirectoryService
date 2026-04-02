using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Pagination;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Positions.Queries;
using DirectoryService.Contracts.Positions;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreatePositionRequest> _positionHandler;
    private readonly IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> _getPositionsQueryHandler;

    public PositionController(
        ICommandHandler<Guid, CreatePositionRequest> positionService,
        IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> getPositionsQueryHandler)
    {
        _positionHandler = positionService;
        _getPositionsQueryHandler = getPositionsQueryHandler;
    }


    [HttpPost("/api/positions")]
    public async Task<EndpointResult<Guid>> CreatePosition(CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        return await _positionHandler.HandleAsync(request, cancellationToken);
    }

    [HttpGet("/api/positions")]
    public async Task<ActionResult<PagedResult<PositionDto>>> GetPositions(
        [FromQuery] GetPositionsQuery query,
        CancellationToken cancellationToken)
    {
        var result =  await _getPositionsQueryHandler.HandleAsync(query, cancellationToken);

        return result;
    }
}