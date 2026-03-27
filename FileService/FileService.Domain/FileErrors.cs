using Shared.Kernel;

namespace FileService.Domain;

public static class FileErrors
{
    public static Error BucketNotFound(string? bucketName = null)
    {
        var name = bucketName ?? string.Empty;
        return Error.NotFound("no.such.bucket", $"Bucket  not found.");
    }

    public static Error UploadNotFound(string? uploadId = null)
    {
        var id = uploadId is null ? string.Empty : $"with ID . ";
        return Error.NotFound("upload.not.found", $"Upload session not found.");
    }

    public static Error ObjectNotFound(string? objectKey = null)
    {
        var key = objectKey is null ? string.Empty : $"with key {objectKey}.";
        return Error.NotFound("no.such.object", $"Object  not found.");
    }
    
    public static Error Forbidden()
    {
        return Error.Failure("access.denied", "You are not allowed to access this resource.");
    }

    public static Error ValidationFailed()
    {
        string message = "Request contains insufficient data";

        return Error.Validation("validation.failed", message);
    }

    public static Error InternalServerError()
    {
        return Error.Failure("internal.server.error", "Internal storage Error.");
    }

    public static Error OperationCanceled()
    {
        return Error.Failure("operation.canceled", "Operation was canceled.");
    }

    public static Error NetworkIssue()
    {
        return Error.Failure(
            "network.issue",
            "Network error occured while trying to access the storage.");
    }

    public static Error Unknown()
    {
        return Error.Failure("unknown.error", "Unknown Error.");
    }

    public static Error HlsProcessingFailed()
    {
        return Error.Failure("hls.processing.failed", $"Video processing failed.");
    }

    public static Error HlsProcessingFailed(string details)
    {
        return Error.Failure("hls.processing.failed", $"Processing failed with error: {details}.");
    }

    public static Error ProcessFailed()
    {
        return Error.Failure("process.failed", $"Process failed.");
    }

    public static Error InvalidFfprobeOutput(string details)
    {
        return Error.Failure("ffprobe.invalid.output", $"Invalid ffprobe: {details}.");
    }

    public static Error Conflict()
    {
        return Error.Conflict($"operation.conflict", "Conflict while changing the state of files.");
    }
}