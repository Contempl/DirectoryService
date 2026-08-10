using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using FileService.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.ReplacePhoto;

public sealed class ReplaceLocationPhotoHandler : ICommandHandler<Guid, ReplacePhotoRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<ReplacePhotoRequest> _validator;
    private readonly ILogger<ReplaceLocationPhotoHandler> _logger;

    public ReplaceLocationPhotoHandler(
        ILocationRepository locationRepository,
        IFileCommunicationService fileCommunicationService,
        ITransactionManager transactionManager,
        IValidator<ReplacePhotoRequest> validator,
        ILogger<ReplaceLocationPhotoHandler> logger)
    {
        _locationRepository = locationRepository;
        _fileCommunicationService = fileCommunicationService;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        ReplacePhotoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var locationResult = await _locationRepository.GetLocationByIdAsync(request.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;
        if (location.Photo is null)
            return LocationPhotoErrors.NotAttached().ToErrors();

        if (location.Photo.AssetId == request.AssetId)
            return LocationPhotoErrors.AssetUnchanged().ToErrors();

        var assetResult = await _fileCommunicationService.GetMediaAsset(request.AssetId, cancellationToken);
        if (assetResult.IsFailure)
            return FileServiceErrorMapper.ToDirectoryError(assetResult.Error).ToErrors();

        var photoResult = LocationPhotoFactory.Create(assetResult.Value);
        if (photoResult.IsFailure)
            return photoResult.Error.ToErrors();

        var replaceResult = location.ReplacePhoto(photoResult.Value);
        if (replaceResult.IsFailure)
            return replaceResult.Error.ToErrors();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation(
            "Replaced photo for location {LocationId} with asset {AssetId}",
            request.LocationId,
            request.AssetId);

        return request.LocationId;
    }
}
