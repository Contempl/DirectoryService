using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.Update;

public class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentRequest>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentRequest> _validator;
    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IValidator<UpdateDepartmentRequest> validator,
        ILogger<UpdateDepartmentHandler> logger,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
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
            return GeneralErrors.ValueIsInvalid("Cannot move department into itself").ToErrors();

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        try
        {
            var departmentResult = await _departmentRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var department = departmentResult.Value;

            if (targetParentId != null)
            {
                var parentResult = await _departmentRepository.GetByIdWithLock(targetParentId.Value, cancellationToken);
                if (parentResult.IsFailure)
                    return parentResult.Error;

                var newParent = parentResult.Value;

                if (newParent.Path.Value == department.Path.Value ||
                    newParent.Path.Value.StartsWith(department.Path.Value + "."))
                {
                    return GeneralErrors.ValueIsInvalid("Cannot move department into its own descendant").ToErrors();
                }

                var moveResult = await _departmentRepository.MoveDepartment(
                    newParent.Id,
                    newParent.Path,
                    department.Path,
                    cancellationToken);

                if (moveResult.IsFailure)
                    return moveResult.Error.ToErrors();
            }
            else
            {
                var moveResult = await _departmentRepository.MoveDepartment(department.Path, cancellationToken);

                if (moveResult.IsFailure)
                    return moveResult.Error.ToErrors();
            }

            transaction.Commit();

            _logger.LogInformation("Department {Id} moved successfully", departmentId);
            return departmentId;
        }
        catch (Exception)
        {
            transaction.Rollback();
            _logger.LogInformation("Department {Id} failed", departmentId);
            return GeneralErrors.Failure().ToErrors();
        }
    }
}