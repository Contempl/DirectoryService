using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using System.Linq.Expressions;
using Shared.Kernel;

namespace FileService.Core;

public interface IMediaAssetsRepository
{
    UnitResult<Error> Add(MediaAsset mediaAsset, CancellationToken cancellationToken = default);
    Task<Result<MediaAsset, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<VideoAsset, Error>> GetVideoBy(
        Expression<Func<VideoAsset, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> RemoveAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
