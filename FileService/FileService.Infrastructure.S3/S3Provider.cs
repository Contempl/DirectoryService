using Amazon.S3;
using Amazon.S3.Model;
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

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger, SemaphoreSlim requestsSemaphore)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(1, _s3Options.MaxConcurrentRequests);
        _logger = logger;
    }

    public async Task<Result<string, Error>> UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken token)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
        };
        
        await _s3Client.PutObjectAsync(request, token);
        return bucketName;
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName, Key = key, ContentType = contentType
            };
            
            var response = await _s3Client.InitiateMultipartUploadAsync(request);

            return response.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");
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

    public Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string, Error>> DownloadFileAsync(StorageKey key, string tempPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken)
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
                    string? result = await _s3Client.GetPreSignedURLAsync(request);
                    return result;
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });
            string[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download urls");
            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag
                {
                    ETag = p.ETag, PartNumber = p.PartNumber
                }).ToList()
            };

            CompleteMultipartUploadResponse response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }


    public async Task<Result<string, Error>> GenerateUploadUrlAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddHours(6),
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
}