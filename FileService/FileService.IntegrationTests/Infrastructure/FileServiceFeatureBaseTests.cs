using FileService.Domain.Assets;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;

namespace FileService.IntegrationTests.Infrastructure;

public abstract class FileServiceFeatureBaseTests : FileServiceBaseTests
{
    protected FileServiceFeatureBaseTests(FileServiceTestWebFactory factory) : base(factory) { }

    protected async Task<MediaAsset> CreateMediaAssetInDb(CancellationToken ct, MediaStatus? status = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var fileName = FileName.Create("test-video.mp4").Value;
            var contentType = ContentType.Create("video/mp4").Value;
            var mediaData = MediaData.Create(fileName, contentType, 1024, 1).Value;
            var owner = MediaOwner.Create("lesson", Guid.NewGuid()).Value;

            var asset = MediaAsset.CreateForUpload(mediaData, AssetType.VIDEO, owner).Value;

            if (status == MediaStatus.UPLOADED)
                asset.MarkUploaded(DateTime.UtcNow);
            else if (status == MediaStatus.FAILED)
                asset.MarkFailed(DateTime.UtcNow);
            else if (status == MediaStatus.DELETED)
                asset.MarkDeleted(DateTime.UtcNow);

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(ct);

            return asset;
        });
    }

    protected async Task<List<MediaAsset>> CreateMediaAssetsInDb(int count, CancellationToken ct)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var assets = new List<MediaAsset>();

            for (var i = 0; i < count; i++)
            {
                var fileName = FileName.Create($"test-video-{i}.mp4").Value;
                var contentType = ContentType.Create("video/mp4").Value;
                var mediaData = MediaData.Create(fileName, contentType, 1024, 1).Value;
                var owner = MediaOwner.Create("lesson", Guid.NewGuid()).Value;

                var asset = MediaAsset.CreateForUpload(mediaData, AssetType.VIDEO, owner).Value;
                assets.Add(asset);
            }

            dbContext.MediaAssets.AddRange(assets);
            await dbContext.SaveChangesAsync(ct);

            return assets;
        });
    }
}
