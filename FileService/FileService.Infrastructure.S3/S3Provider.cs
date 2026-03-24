using Amazon.S3;
using Amazon.S3.Model;
using FileService.Core;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
    }

    public async Task UploadFileAsync(Stream stream, string bucketName, string key, string contentType, CancellationToken token)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
        };
        
        
        await _s3Client.PutObjectAsync(request, token);
    }

    public async Task<string> GenerateDownloadUrlAsync(Stream stream, string bucketName, string key)
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
    
    public async Task<string> GenerateUploadUrlAsync(string bucketName, string key)
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
}