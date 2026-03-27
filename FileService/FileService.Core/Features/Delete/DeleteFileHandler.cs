using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.Delete;

public class DeleteFileHandler : ICommandHandler<Guid, DeleteFileRequest>
{
    private readonly IMediaAssetsRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DeleteFileRequest> _validator;
    private readonly ILogger<DeleteFileHandler> _logger;

    public DeleteFileHandler(IMediaAssetsRepository mediaAssetRepository, 
        IS3Provider s3Provider, 
        IValidator<DeleteFileRequest> validator,
        ILogger<DeleteFileHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(DeleteFileRequest request, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validResult.IsValid)
        {
            _logger.LogInformation("Invalid request: {request}", request);
            return validResult.ToErrors();
        }
        
        var fileId = request.FileId;
        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(fileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
        {
            _logger.LogInformation("Media asset not found: {fileId}", fileId);
            return mediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = mediaAssetResult.Value;

        var deletionResult = await _s3Provider.DeleteFileAsync(mediaAsset.RawKey, cancellationToken);
        if (deletionResult.IsFailure)
        {
            _logger.LogInformation("Deletion failed: {fileId}", fileId);
            return deletionResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Deleted file with id:{fileId}", fileId);

        return fileId;
    }
}