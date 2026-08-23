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
    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public StorageKey HlsRootKey { get; protected set; }

    private VideoAsset() { }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus mediaStatus,
        MediaOwner owner,
        StorageKey key)
        : base (id, mediaData, mediaStatus, AssetType.VIDEO, owner, key)
    {
        
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


    public UnitResult<Error> CompleteProcessing(DateTime timestamp)
    {
        var finalKeyResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);
        
        if (finalKeyResult.IsFailure)
            return finalKeyResult.Error;

        var finalKey = finalKeyResult.Value;
        
        var result = MarkReady(finalKey, timestamp);

        if (result.IsFailure)
            return result.Error;
        
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

        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            key.Value);
    }
}
