using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Constants;
using Shared.Kernel;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.Update;

public class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentRequest>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentRequest> _validator;
    private readonly HybridCache _cache;
    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IValidator<UpdateDepartmentRequest> validator,
        ILogger<UpdateDepartmentHandler> logger,
        ITransactionManager transactionManager, 
        HybridCache cache)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentId = request.DepartmentId;
        var targetParentId = request.ParentId;

        if (targetParentId == departmentId)
            return DepartmentMoveErrors.Cycle().ToErrors();

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        try
        {
            var departmentResult = await _departmentRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
                return DepartmentMoveErrors.DepartmentNotFound(departmentId).ToErrors();

            var department = departmentResult.Value;
            
            if (!department.IsActive)
                return DepartmentMoveErrors.DepartmentNotFound(departmentId).ToErrors();

            if (targetParentId != null)
            {
                var parentLookupResult = await _departmentRepository.GetByIdAsNoTrackingAsync(
                    targetParentId.Value,
                    cancellationToken);

                if (parentLookupResult.IsFailure)
                    return DepartmentMoveErrors.DepartmentNotFound(targetParentId.Value).ToErrors();

                if (!parentLookupResult.Value.IsActive)
                    return DepartmentMoveErrors.ParentDeleted().ToErrors();

                var parentResult = await _departmentRepository.GetByIdWithLock(targetParentId.Value, cancellationToken);
                if (parentResult.IsFailure)
                    return DepartmentMoveErrors.ParentDeleted().ToErrors();

                var newParent = parentResult.Value;

                var descendantIds = await _departmentRepository.GetDescendantIdsAsync(
                    department.Id,
                    cancellationToken);

                if (descendantIds.Contains(newParent.Id))
                {
                    return DepartmentMoveErrors.Cycle().ToErrors();
                }

                var moveResult = await _departmentRepository.MoveDepartment(
                    department.Id,
                    newParent.Id,
                    newParent.Path,
                    department.Path,
                    cancellationToken);

                if (moveResult.IsFailure)
                    return moveResult.Error.ToErrors();
            }
            else
            {
                var moveResult = await _departmentRepository.MoveDepartment(
                    department.Id, department.Path, cancellationToken);

                if (moveResult.IsFailure)
                    return moveResult.Error.ToErrors();
            }

            transaction.Commit();

            try
            {
                await _cache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_KEY, cancellationToken);
            }
            catch (Exception cacheException)
            {
                _logger.LogWarning(
                    cacheException,
                    "Department {Id} moved, but department cache invalidation failed",
                    departmentId);
            }
            
            _logger.LogInformation("Department {Id} moved successfully", departmentId);
            
            return departmentId;
        }
        catch (Exception exception)
        {
            transaction.Rollback();
            _logger.LogError(exception, "Department {Id} move failed", departmentId);
            return GeneralErrors.Failure().ToErrors();
        }
    }
}
