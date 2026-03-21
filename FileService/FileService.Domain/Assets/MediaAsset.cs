using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id  { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!; 

    public AssetType AssetType { get; protected set; } 

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; protected set; } =  DateTime.UtcNow;
    
    public StorageKey RawKey { get; protected set; } = null!;

    public StorageKey FinalKey { get; set; } = null!;

    public MediaOwner Owner { get; protected set; } = null!;
    
    public MediaStatus Status { get; protected set; }
    
    protected  MediaAsset()
    {}

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        MediaOwner owner,
        StorageKey key)
    {
        Id = id;
        MediaData = mediaData;
        AssetType = assetType;
        Status = status;
        Owner = owner;
        RawKey = key;
    }

    protected UnitResult<Error> MarkUploaded(DateTime timestamp)
    {
        if (Status == MediaStatus.UPLOADED)
            return UnitResult.Success<Error>();
        
        Status = MediaStatus.UPLOADED;
        
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }

    protected UnitResult<Error> MarkReady(StorageKey finalKey, DateTime timestamp)
    {
        if (Status == MediaStatus.READY)
            return UnitResult.Success<Error>();

        Status = MediaStatus.READY;
        
        UpdatedAt =  timestamp;
        
        return UnitResult.Success<Error>();
    }

    protected UnitResult<Error> MarkFailed(DateTime timestamp)
    {
        if (Status == MediaStatus.FAILED)
            return UnitResult.Success<Error>();
        
        Status = MediaStatus.FAILED;
        
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }

    protected UnitResult<Error> MarkDeleted(DateTime timestamp)
    {
        if (Status == MediaStatus.DELETED)
            return UnitResult.Success<Error>();

        Status = MediaStatus.DELETED;
        
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }
    
}