using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.DeletePhoto;

public sealed class DeleteLocationPhotoHandler : ICommandHandler<Guid, DeletePhotoRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<DeletePhotoRequest> _validator;
    private readonly ILogger<DeleteLocationPhotoHandler> _logger;

    public DeleteLocationPhotoHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        IValidator<DeletePhotoRequest> validator,
        ILogger<DeleteLocationPhotoHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        DeletePhotoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var locationResult = await _locationRepository.GetLocationByIdAsync(request.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var removeResult = locationResult.Value.RemovePhoto();
        if (removeResult.IsFailure)
            return removeResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation("Removed photo from location {LocationId}", request.LocationId);

        return request.LocationId;
    }
}
