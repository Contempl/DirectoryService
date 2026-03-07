using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Delete;

public class DeleteLocationHandler : ICommandHandler<Guid, DeleteLocationRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteLocationHandler> _logger;

    public DeleteLocationHandler(ILocationRepository locationRepository, 
        ITransactionManager transactionManager,
        ILogger<DeleteLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(DeleteLocationRequest request, CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError("failed to begin transaction.");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;

        var existingLocation = await _locationRepository.GetLocationByIdAsync(request.LocationId, cancellationToken);
        if (existingLocation.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to fetch location to delete.");
            
            return existingLocation.Error.ToErrors();
        }
        
        var locationToDelete = existingLocation.Value;

        var deletionResult = locationToDelete.SoftDelete();
        if (deletionResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to delete location.");
            
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

        return request.LocationId;
    }
}