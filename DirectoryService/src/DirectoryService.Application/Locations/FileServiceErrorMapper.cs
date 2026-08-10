using DsError = DirectoryService.Domain.Shared.Error;
using FsError = Shared.Kernel.Error;

namespace DirectoryService.Application.Locations;

internal static class FileServiceErrorMapper
{
    public static DsError ToDirectoryError(FsError error) => error.Type switch
    {
        Shared.Kernel.ErrorType.NOT_FOUND => DsError.NotFound(error.Code, error.Message),
        Shared.Kernel.ErrorType.VALIDATION => DsError.Validation(error.Code, error.Message, error.InvalidField),
        Shared.Kernel.ErrorType.CONFLICT => DsError.Conflict(error.Code, error.Message),
        _ => DsError.Failure(error.Code, error.Message)
    };
}
