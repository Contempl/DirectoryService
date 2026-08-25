using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.Upload;

public class UploadFileHandler : ICommandHandler<Guid, UploadFileCommand>
{
    private readonly IMediaAssetsRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<UploadFileCommand> _validator;
    private readonly ILogger<UploadFileHandler> _logger;

    public UploadFileHandler(
        IMediaAssetsRepository mediaAssetRepository,
        IS3Provider s3Provider,
        IValidator<UploadFileCommand> validator,
        ILogger<UploadFileHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Invalid request: {command}", command);
            return validationResult.ToErrors();
        }
        
        var request = command.Request;
        
        var assetType = request.AssetType.ToAssetType();
        
        var file = request.FormFile;
        
        var fileName = FileName.Create(file.FileName).Value;
        
        var contentType = ContentType.Create(file.ContentType).Value;
        
        long size = file.Length;
        
        var mediaDataResult = MediaData.Create(fileName, contentType, size, 1);
        
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error.ToErrors();
        
        var mediaData = mediaDataResult.Value;
        var owner = MediaOwner.Create(request.Context, request.ContextId);
        
        var mediaAssetResult = MediaAsset.CreateForUpload(mediaData, assetType, owner.Value);

        if (mediaAssetResult.IsFailure)
        {
            _logger.LogInformation("Failed to create file {fileName}", fileName);
            return mediaAssetResult.Error.ToErrors();
        }
            
        
        var mediaAsset = mediaAssetResult.Value;
        var addResult = _mediaAssetRepository.Add(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
        {
            _logger.LogInformation("Failed to upload file {fileName}", fileName);
            return addResult.Error.ToErrors();
        }
        
        var uploadResult = await _s3Provider.UploadFileAsync(
            mediaAsset.RawKey,
            file.OpenReadStream(),
            mediaData,
            cancellationToken);
        if (uploadResult.IsFailure)
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Failed to upload file {fileName}", fileName);
            return uploadResult.Error.ToErrors();
        }
        
        mediaAsset.MarkUploaded(DateTime.UtcNow);
        
        await _mediaAssetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Uploaded file {file.Name}");

        return mediaAsset.Id;
    }
}
