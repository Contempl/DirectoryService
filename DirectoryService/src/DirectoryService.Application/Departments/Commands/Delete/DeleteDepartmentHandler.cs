using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.Delete;

public class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentRequest>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentRepository departmentRepository, 
        ILogger<DeleteDepartmentHandler> logger, 
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(DeleteDepartmentRequest request, CancellationToken cancellationToken)
    {
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure) return transactionResult.Error.ToErrors();
        using var transaction = transactionResult.Value;
        
        var departmentResult = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            _logger.LogError($"Department with id: {request.DepartmentId} not found");
            return GeneralErrors.ValueIsInvalid("Department doesn't exist.").ToErrors();
        }

        var department = departmentResult.Value;
        var oldPath = department.Path;
        
        var deletionResult = department.SoftDelete();
        if (deletionResult.IsFailure)
            return deletionResult.Error.ToErrors();
        
        var pathIdentifierUpdateResult = department.ChangePathAfterSoftDelete();
        if (pathIdentifierUpdateResult.IsFailure)
            return pathIdentifierUpdateResult.Error.ToErrors();
        
        var updateResult = await _departmentRepository.UpdateTreePathsAsync(oldPath, pathIdentifierUpdateResult.Value, cancellationToken);
        if (updateResult.IsFailure)
        {
            transaction.Rollback();
            _logger.LogError($"Failed to update paths while deleting department. Id: {request.DepartmentId}.");
            
            return updateResult.Error.ToErrors();
        }
        
        var deleteOrphanLocationsResult = await _departmentRepository.DeactivateOrphanedLocationsAsync(department.Id, cancellationToken);
        if (deleteOrphanLocationsResult.IsFailure)
        {
            transaction.Rollback();
            _logger.LogError($"Failed to delete orphan locations while deactivating department. Id: {request.DepartmentId}.");
            
            return deleteOrphanLocationsResult.Error.ToErrors();
        }
        
            
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transaction.Rollback();
            _logger.LogError($"Department with id: {request.DepartmentId} failed to delete");
            
            return saveChangesResult.Error.ToErrors();
        }
        
        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
        {
            transaction.Rollback();
            _logger.LogError("Failed to commit the transaction.");
            
            return commitResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Department {Id} soft deleted.", department.Id);
        return department.Id;
    }
}