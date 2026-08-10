using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using Shared.Kernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Update;

public class UpdatePositionHandler : ICommandHandler<Position, UpdatePositionCommand>
{
    private readonly IValidator<UpdatePositionCommand> _validator;
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdatePositionHandler> _logger;

    public UpdatePositionHandler(IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        IValidator<UpdatePositionCommand> validator,
        ILogger<UpdatePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Position, Errors>> HandleAsync(UpdatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Failed to validate position.");
            return validationResult.ToErrors();
        }
        
        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Failed to start transaction.");
            return beginTransaction.Error.ToErrors();
        }

        using var transactionScope = beginTransaction.Value;
        
        var positionResult = await _positionRepository.GetPositionById(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            _logger.LogInformation("Failed to fetch position.");
            return positionResult.Error.ToErrors();
        }

        var position = positionResult.Value;

        var newName = Name.Create(command.Request.Name);
        if (newName.IsFailure)
        {
            _logger.LogInformation("Failed to create new name for position.");
            return newName.Error.ToErrors();
        }

        var positionUpdate = position.Update(newName.Value, command.Request.Description);
        if (positionUpdate.IsFailure)
        {
            _logger.LogInformation("Failed to update position.");
            return positionUpdate.Error;
        }
        
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to save changes via transaction.");
            
            return saveChangesResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Failed to commit  transaction.");
            
            return commitResult.Error.ToErrors();
        }

        return position;
    }
    
}
