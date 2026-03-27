using Amazon.S3;
using FileService.Domain;
using Shared.Kernel;

namespace FileService.Infrastructure;

public static class S3ErrorMapper
{
    public static Error ToError(Exception ex) => ex switch
    {
        AmazonS3Exception { ErrorCode: "NoSuchBucker"}
            => FileErrors.BucketNotFound(),
        AmazonS3Exception { ErrorCode: "AccessDenied" }
            => FileErrors.Forbidden(),

        AmazonS3Exception { ErrorCode: "InvalidObjectState" }
            => FileErrors.Conflict(),

        HttpRequestException
            => FileErrors.NetworkIssue(),

        OperationCanceledException
            => FileErrors.OperationCanceled(),

        _ => FileErrors.InternalServerError(),
    };
}