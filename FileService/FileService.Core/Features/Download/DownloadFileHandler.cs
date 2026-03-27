using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.Download;

public class DownloadFileHandler : IQueryHandler<DownloadFileRequest, Result<string, Error>>
{
    private readonly IMediaAssetsRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<DownloadFileHandler> _logger;

    public DownloadFileHandler(
        IMediaAssetsRepository mediaAssetRepository, 
        IS3Provider s3Provider, 
        ILogger<DownloadFileHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<string, Error>> HandleAsync(DownloadFileRequest request, CancellationToken cancellationToken)
    {
        var fileId = request.FileId;
        var mediaAssetResult = await _mediaAssetRepository.GetByIdAsync(fileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var mediaAsset = mediaAssetResult.Value;

        var downloadResult = await _s3Provider.DownloadFileAsync(
            mediaAsset.RawKey,
            cancellationToken);
        
        if (downloadResult.IsFailure)
        {
            return downloadResult.Error;
        }

        _logger.LogInformation("Download file with id:{fileId}", fileId);

        return downloadResult.Value;
    }
}