using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Pagination;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Positions.Queries;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreatePositionRequest> _positionHandler;
    private readonly IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> _getPositionsQueryHandler;
    private readonly IQueryHandler<Guid, Result<PositionDto, Error>> _getPositionQueryHandler;

    public PositionController(
        ICommandHandler<Guid, CreatePositionRequest> positionService,
        IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> getPositionsQueryHandler,
        IQueryHandler<Guid, Result<PositionDto, Error>> getPositionQueryHandler)
    {
        _positionHandler = positionService;
        _getPositionsQueryHandler = getPositionsQueryHandler;
        _getPositionQueryHandler = getPositionQueryHandler;
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

    [HttpGet("/api/positions/{positionId}")]
    public async Task<ActionResult<PositionDto>> GetPositionById(
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var result = await _getPositionQueryHandler.HandleAsync(positionId, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        
        return Ok(result.Value);
    }
}