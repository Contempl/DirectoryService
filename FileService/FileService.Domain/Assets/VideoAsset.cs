using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.Domain.Assets;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120; 
    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_PREFIX = "hls";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string STREAM_PLAYLIST_PATTERN = "stream_%v.m3u8";
    public const string SEGMENT_FILE_PATTERN = "segment_%v_%03d.ts";
    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];
    public VideoMetadata? Metadata { get; private set; }

    public StorageKey HlsRootKey { get; private set; } = null!;

    private VideoAsset() { }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus mediaStatus,
        MediaOwner owner,
        StorageKey key,
        StorageKey hlsRootKey)
        : base (id, mediaData, mediaStatus, AssetType.VIDEO, owner, key)
    {
        HlsRootKey = hlsRootKey;
    }

    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.MediaType != MediaType.VIDEO)
        {
            return Error.Validation("video.invalid.content-type", $"File content type must be video");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        }

        return UnitResult.Success<Error>();
    }


    public UnitResult<Error> CompleteProcessing()
    {
        if (Status != MediaStatus.PROCESSING)
        {
            return Error.Validation(
                "video.invalid.status",
                "Can only complete processing from PROCESSING status");
        }

        if (FinalKey is null)
        {
            var masterPlaylistKey = GetHlsMasterPlaylistKey();
            if (masterPlaylistKey.IsFailure)
                return masterPlaylistKey.Error;

            FinalKey = masterPlaylistKey.Value;
        }

        return MarkReady();
    }

    public override bool RequiresProcessing() => true;

    public Result<StorageKey, Error> GetHlsRootKey()
    {
        return StorageKey.Create(LOCATION, HLS_PREFIX, Id.ToString());
    }

    public Result<StorageKey, Error> GetHlsMasterPlaylistKey()
    {
        var hlsRoot = GetHlsRootKey();
        if (hlsRoot.IsFailure)
            return hlsRoot.Error;

        return hlsRoot.Value.AppendSegment(MASTER_PLAYLIST_NAME);
    }

    public UnitResult<Error> SetHlsMasterPlaylistKey(StorageKey value)
    {
        if (Status != MediaStatus.PROCESSING)
        {
            return Error.Validation(
                "video.invalid.status",
                "Can only set processed data during processing");
        }

        FinalKey = value;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public static Result<VideoAsset, Error> CreateMediaForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        UnitResult<Error> validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> key = StorageKey.Create(LOCATION, RAW_PREFIX, id.ToString());
        if (key.IsFailure)
            return key.Error;

        Result<StorageKey, Error> hlsRootKey = StorageKey.Create(LOCATION, HLS_PREFIX, id.ToString());
        if (hlsRootKey.IsFailure)
            return hlsRootKey.Error;

        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            key.Value,
            hlsRootKey.Value);
    }

    public void SetMetadata(VideoMetadata metadata)
    {
        Metadata = metadata;
    }

    public UnitResult<Error> StartProcessing()
    {
        if (Status == MediaStatus.PROCESSING)
            return UnitResult.Success<Error>();

        if (Status != MediaStatus.UPLOADED)
            return Error.Validation("asset.invalid.status.transaction", "Can only start processing from UPLOADED status");

        if (!RequiresProcessing())
            return Error.Validation("asset.processing.not.required", "This asset type does not require processing");

        Status = MediaStatus.PROCESSING;
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> PrepareProcessingRetry()
    {
        // FS-12: Возвращаем только FAILED-видео в исходную точку повторного pipeline.
        if (Status != MediaStatus.FAILED)
            return Error.Validation(
                "asset.invalid.retry.status",
                "Can only prepare processing retry from FAILED status");

        Status = MediaStatus.UPLOADED;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}
