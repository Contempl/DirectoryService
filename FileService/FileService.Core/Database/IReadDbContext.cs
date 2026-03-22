using FileService.Domain.Assets;

namespace FileService.Core.Database;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsQuery { get; }
}