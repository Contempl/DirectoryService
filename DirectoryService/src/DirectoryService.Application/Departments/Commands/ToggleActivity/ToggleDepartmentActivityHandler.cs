using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Constants;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.ToggleActivity;

public class ToggleDepartmentActivityHandler : ICommandHandler<Guid, ToggleDepartmentActivityRequest>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _hybridCache;
    private readonly ILogger<ToggleDepartmentActivityHandler> _logger;
    private readonly IValidator<ToggleDepartmentActivityRequest> _validator;

    public ToggleDepartmentActivityHandler(IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        HybridCache hybridCache,
        ILogger<ToggleDepartmentActivityHandler> logger,
        IValidator<ToggleDepartmentActivityRequest> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _hybridCache = hybridCache;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(ToggleDepartmentActivityRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentResult = await _departmentRepository.GetByIdForActivityAsync(request.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        var department = departmentResult.Value;

        if (department.IsActive && !request.IsActive)
        {
            var activeDescendantIds = await _departmentRepository.GetDescendantIdsAsync(department.Id, cancellationToken);
            if (activeDescendantIds.Count > 0)
                return Error.Conflict("department.activity.active_descendants", "Active child departments detected.").ToErrors();
        }

        var activityResult = request.IsActive
            ? department.Activate()
            : department.Deactivate();

        if (activityResult.IsFailure)
            return activityResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            _logger.LogError("Failed to save changes.");
            return saveResult.Error.ToErrors();
        }

        try
        {
            await _hybridCache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_KEY, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Department {DepartmentId} activity changed, but cache invalidation failed.",
                department.Id);
        }


        return department.Id;
    }
}
