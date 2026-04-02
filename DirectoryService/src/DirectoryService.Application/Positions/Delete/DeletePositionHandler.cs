using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Delete;

public class DeletePositionHandler : ICommandHandler<Guid, DeletePositionRequest>
{
    public readonly IPositionRepository _positionRepository;
    public readonly ITransactionManager _transactionManager;
    public readonly ILogger<DeletePositionHandler> _logger;

    public DeletePositionHandler(
        IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        ILogger<DeletePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(DeletePositionRequest request, CancellationToken cancellationToken)
    {
        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return GeneralErrors.Failure().ToErrors();
        }

        using var transactionScope = beginTransaction.Value;

        var positionResult = await _positionRepository.GetPositionById(request.PositionId, cancellationToken);
        if (positionResult.IsFailure)
            return positionResult.Error.ToErrors();

        var position = positionResult.Value;

        var deletionResult = position.SoftDelete();
        if (deletionResult.IsFailure)
        {
            _logger.LogInformation("Couldn't delete position.");
            return deletionResult.Error.ToErrors();
        }
        
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to save changes while deleting location.");
            
            return saveChangesResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to commit changes in the transaction.");
            
            return commitResult.Error.ToErrors();
        }

        return position.Id;
    }
}