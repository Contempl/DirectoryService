using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.Core;

public interface IVideoProcessingRepository
{
    Task<Result<VideoProcessing, Error>> GetByAsync(
        Expression<Func<VideoProcessing, bool>> predicate,
        CancellationToken cancellationToken);

    Result<Guid, Error> Add(VideoProcessing videoProcessing);
}