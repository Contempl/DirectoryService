using Amazon.S3;
using Amazon.S3.Model;
using S3AbortRequest = Amazon.S3.Model.AbortMultipartUploadRequest;
using S3CompleteRequest = Amazon.S3.Model.CompleteMultipartUploadRequest;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core;
using FileService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Kernel;

namespace FileService.Infrastructure;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;
    private readonly ILogger<S3Provider> _logger;
    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests, _s3Options.MaxConcurrentRequests);
        _logger = logger;
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType
            };

            var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);
            return response.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                ContentType = mediaData.ContentType.Value
            };

            var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);
            return response.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateChunkUploadUrl(StorageKey key, string uploadId, int partNumber)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                Verb = HttpVerb.PUT,
                PartNumber = partNumber,
                UploadId = uploadId,
                Expires = DateTime.UtcNow.AddMinutes(_s3Options.UploadUrlExpirationMinutes),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chunk upload URL for part {PartNumber}", partNumber);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(
        StorageKey key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expires = DateTime.UtcNow.AddMinutes(_s3Options.UploadUrlExpirationMinutes);

            var tasks = Enumerable.Range(1, totalChunks).Select(async partNumber =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = key.Location,
                        Key = key.Value,
                        Verb = HttpVerb.PUT,
                        PartNumber = partNumber,
                        UploadId = uploadId,
                        Expires = expires,
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    var url = await _s3Client.GetPreSignedURLAsync(request);
                    return new ChunkUploadUrl { PartNumber = partNumber, UploadUrl = url };
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return Result.Success<IReadOnlyList<ChunkUploadUrl>, Error>(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chunk upload URLs");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> partETags,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new S3CompleteRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                UploadId = uploadId,
                PartETags = partETags
                    .Select(p => new PartETag { PartNumber = p.PartNumber, ETag = p.ETag })
                    .ToList()
            };

            await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload {UploadId}", uploadId);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new S3AbortRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                UploadId = uploadId
            };

            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting multipart upload {UploadId}", uploadId);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<MultipartUploadEntry>, Error>> ListMultipartUploadAsync(
        string bucketName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListMultipartUploadsRequest { BucketName = bucketName };
            var response = await _s3Client.ListMultipartUploadsAsync(request, cancellationToken);

            var entries = response.MultipartUploads
                .Select(u => new MultipartUploadEntry(u.Key, u.UploadId, u.Initiated ?? DateTime.MinValue))
                .ToList();

            return Result.Success<IReadOnlyList<MultipartUploadEntry>, Error>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing multipart uploads in bucket {BucketName}", bucketName);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(Stream stream, string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now.AddHours(_s3Options.DownloadUrlExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            };

            var response = await _s3Client.GetPreSignedURLAsync(request);
            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey key,
        Stream stream,
        MediaData mediaData,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = key.Location,
            Key = key.Value,
            InputStream = stream,
            ContentType = mediaData.ContentType.Value,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey key,
        FileStream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                InputStream = fileStream,
                ContentType = contentType ?? "application/octet-stream"
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to {Key}", key.Value);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = key.Location,
                Key = key.Value,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> DeleteFileAsync(
        StorageKey key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = key.Location,
                Key = key.Value
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Key}", key.Value);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> keys,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = keys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Location,
                        Key = storageKey.Value,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.Now.AddHours(_s3Options.DownloadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    return await _s3Client.GetPreSignedURLAsync(request);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            string[] results = await Task.WhenAll(tasks);
            return Result.Success<IReadOnlyList<string>, Error>(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URLs");
            return S3ErrorMapper.ToError(ex);
        }
    }
}
