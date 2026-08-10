using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using Shared.Kernel;
using FluentValidation;

namespace DirectoryService.Application.Locations.BulkActivityUpdate;

public class BulkUpdateLocationsActivityHandler : 
    ICommandHandler<BulkUpdateLocationsActivityResult, BulkUpdateLocationsActivityRequest>
{
    private readonly IValidator<BulkUpdateLocationsActivityRequest> _validator;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;

    public BulkUpdateLocationsActivityHandler(IValidator<BulkUpdateLocationsActivityRequest> validator,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager)
    {
        _validator = validator;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<BulkUpdateLocationsActivityResult, Errors>> HandleAsync(BulkUpdateLocationsActivityRequest request, 
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var requestedIds = request.LocationIds.Distinct().ToArray();

        var locations = await _locationRepository.GetLocationsByIdsAsync(
            requestedIds,
            cancellationToken);

        var foundIds = locations
            .Select(location => location.Id)
            .ToHashSet();

        var itemErrors = requestedIds
            .Where(id => !foundIds.Contains(id))
            .Select(id => new BulkLocationError(id, "Location not found."))
            .ToList();

        var transactionResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        foreach (var location in locations)
        {
            var activityResult = request.IsActive
                ? location.Restore()
                : location.SoftDelete();

            if (activityResult.IsFailure)
            {
                transaction.Rollback();
                return activityResult.Error.ToErrors();
            }
        }

        var saveResult =
            await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
        {
            transaction.Rollback();
            return commitResult.Error.ToErrors();
        }

        return new BulkUpdateLocationsActivityResult(
            ProcessedCount: locations.Count,
            Errors: itemErrors);
    }
}

