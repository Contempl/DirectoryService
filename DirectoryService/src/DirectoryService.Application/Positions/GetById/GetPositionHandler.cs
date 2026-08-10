using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;
using Shared.Kernel;

namespace DirectoryService.Application.Positions.GetById;

public class GetPositionHandler : IQueryHandler<Guid, Result<PositionDto, Error>>
{
    private readonly IPositionRepository _positionRepository;

    public GetPositionHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<Result<PositionDto, Error>> HandleAsync(Guid positionId, CancellationToken cancellationToken)
    {
        var positionResult = await _positionRepository.GetPositionById(positionId, cancellationToken);
        if (positionResult.IsFailure)
            return positionResult.Error;

        var position = positionResult.Value;

        var result = new PositionDto
        {
            Id = position.Id,
            Name = position.Name.Value,
            Description = position.Description ?? string.Empty,
            IsActive = position.IsActive,
            CreatedAt = position.CreatedAt,
            UpdatedAt = position.UpdatedAt
        };
        
        return result;
    }
}
