using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using FileService.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.AttachPhoto;

public sealed class AttachLocationPhotoHandler : ICommandHandler<Guid, AttachPhotoRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<AttachPhotoRequest> _validator;
    private readonly ILogger<AttachLocationPhotoHandler> _logger;

    public AttachLocationPhotoHandler(
        ILocationRepository locationRepository,
        IFileCommunicationService fileCommunicationService,
        ITransactionManager transactionManager,
        IValidator<AttachPhotoRequest> validator,
        ILogger<AttachLocationPhotoHandler> logger)
    {
        _locationRepository = locationRepository;
        _fileCommunicationService = fileCommunicationService;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        AttachPhotoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var locationResult = await _locationRepository.GetLocationByIdAsync(request.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;
        if (location.Photo is not null)
            return LocationPhotoErrors.AlreadyAttached().ToErrors();

        var assetResult = await _fileCommunicationService.GetMediaAsset(request.AssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();

        var photoResult = LocationPhotoFactory.Create(assetResult.Value);
        if (photoResult.IsFailure)
            return photoResult.Error.ToErrors();

        var attachResult = location.AttachPhoto(photoResult.Value);
        if (attachResult.IsFailure)
            return attachResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation(
            "Attached photo asset {AssetId} to location {LocationId}",
            request.AssetId,
            request.LocationId);

        return request.LocationId;
    }
}
