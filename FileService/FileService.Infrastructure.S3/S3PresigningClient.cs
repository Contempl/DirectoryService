using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure;

internal sealed class S3PresigningClient : IDisposable
{
    public S3PresigningClient(IOptions<S3Options> options)
    {
        var s3Options = options.Value;
        var endpoint = string.IsNullOrWhiteSpace(s3Options.PublicEndpoint)
            ? s3Options.Endpoint
            : s3Options.PublicEndpoint;
        var endpointUri = new Uri(endpoint, UriKind.Absolute);

        Protocol = endpointUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            ? Protocol.HTTPS
            : Protocol.HTTP;

        Client = new AmazonS3Client(
            s3Options.AccessKey,
            s3Options.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = endpoint,
                UseHttp = Protocol == Protocol.HTTP,
                ForcePathStyle = true,
            });
    }

    public IAmazonS3 Client { get; }

    public Protocol Protocol { get; }

    public void Dispose() => Client.Dispose();
}
