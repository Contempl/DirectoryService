using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Pagination;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Positions.Delete;
using DirectoryService.Application.Positions.Queries;
using DirectoryService.Application.Positions.Update;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreatePositionRequest> _positionHandler;
    private readonly ICommandHandler<Guid, DeletePositionRequest> _deletePositionHandler;
    private readonly ICommandHandler<Position, UpdatePositionCommand> _updatePositionHandler;
    private readonly IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> _getPositionsQueryHandler;
    private readonly IQueryHandler<Guid, Result<PositionDto, Error>> _getPositionQueryHandler;

    public PositionController(
        ICommandHandler<Guid, CreatePositionRequest> positionService,
        IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>> getPositionsQueryHandler,
        IQueryHandler<Guid, Result<PositionDto, Error>> getPositionQueryHandler,
        ICommandHandler<Guid, DeletePositionRequest> deletePositionHandler,
        ICommandHandler<Position, UpdatePositionCommand> updatePositionHandler)
    {
        _positionHandler = positionService;
        _getPositionsQueryHandler = getPositionsQueryHandler;
        _getPositionQueryHandler = getPositionQueryHandler;
        _deletePositionHandler = deletePositionHandler;
        _updatePositionHandler = updatePositionHandler;
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

    [HttpDelete("/api/positions/{positionId}")]
    public async Task<EndpointResult<Guid>> DeletePosition(
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var request =  new DeletePositionRequest(positionId);
        
        var result = await _deletePositionHandler.HandleAsync(request, cancellationToken);
        
        return result;
    }

    [HttpPut("/api/positions/{positionId}")]
    public async Task<EndpointResult<Position>> UpdatePosition(
        [FromRoute] Guid positionId,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePositionCommand(positionId, request);
        
        return await _updatePositionHandler.HandleAsync(command, cancellationToken);
    }
}
