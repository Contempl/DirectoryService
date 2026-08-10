using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.AttachPhoto;
using DirectoryService.Application.Locations.DeletePhoto;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Locations.ReplacePhoto;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FileService.Contracts.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public sealed class LocationPhotoTests : DirectoryBaseTests
{
    public LocationPhotoTests(DirectoryServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task AttachPhoto_WithReadyPreview_PersistsLocalMetadata()
    {
        // Arrange
        var locationId = await CreateLocation();
        var asset = CreateAsset(status: "ready", assetType: "preview");
        FileService.Add(asset);

        // Act
        var result = await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, asset.Id), CancellationToken.None));

        // Assert
        Assert.True(result.IsSuccess);
        var photo = await GetPhoto(locationId);
        Assert.NotNull(photo);
        Assert.Equal(asset.Id, photo.AssetId);
        Assert.Equal(asset.FileInfo.FileName, photo.FileName);
        Assert.Equal(asset.FileInfo.ContentType, photo.ContentType);
        Assert.Equal(asset.FileInfo.Size, photo.Size);
    }

    [Fact]
    public async Task ReplaceThenDeletePhoto_AreExplicitOperations()
    {
        // Arrange
        var locationId = await CreateLocation();
        var original = CreateAsset(status: "ready", assetType: "preview");
        var replacement = CreateAsset(status: "ready", assetType: "preview");
        FileService.Add(original);
        FileService.Add(replacement);

        // Act
        await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, original.Id), CancellationToken.None));
        var replaceResult = await Handle<ICommandHandler<Guid, ReplacePhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new ReplacePhotoRequest(locationId, replacement.Id), CancellationToken.None));

        // Assert
        Assert.True(replaceResult.IsSuccess);
        Assert.Equal(replacement.Id, await ExecuteInDb(db => db.Locations
            .Where(location => location.Id == locationId)
            .Select(location => location.Photo!.AssetId)
            .SingleAsync()));

        // Act
        var deleteResult = await Handle<ICommandHandler<Guid, DeletePhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new DeletePhotoRequest(locationId), CancellationToken.None));

        // Assert
        Assert.True(deleteResult.IsSuccess);
        Assert.Null(await GetPhoto(locationId));
    }

    [Fact]
    public async Task AttachPhoto_WithUnreadyOrWrongAsset_IsRejected()
    {
        // Arrange
        var locationId = await CreateLocation();
        var unready = CreateAsset(status: "uploading", assetType: "preview");
        var wrongType = CreateAsset(status: "ready", assetType: "video", contentType: "video/mp4");
        FileService.Add(unready);
        FileService.Add(wrongType);

        // Act
        var unreadyResult = await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, unready.Id), CancellationToken.None));
        var wrongTypeResult = await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, wrongType.Id), CancellationToken.None));

        // Assert
        Assert.True(unreadyResult.IsFailure);
        Assert.True(wrongTypeResult.IsFailure);
        Assert.Null(await GetPhoto(locationId));
    }

    [Fact]
    public async Task GetLocations_WhenFileServiceUnavailable_ReturnsEntityWithStaleMetadata()
    {
        // Arrange
        var locationId = await CreateLocation();
        var asset = CreateAsset(status: "ready", assetType: "preview");
        FileService.Add(asset);
        await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, asset.Id), CancellationToken.None));
        FileService.IsUnavailable = true;

        // Act
        var result = await Handle<IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>, PagedResult<LocationDto>>(
            handler => handler.HandleAsync(new GetLocationsQuery(null, null), CancellationToken.None));

        // Assert
        var location = Assert.Single(result.Items);
        Assert.Equal(locationId, location.Id);
        Assert.NotNull(location.Photo);
        Assert.Equal(LocationPhotoStatuses.TemporarilyUnavailable, location.Photo.Status);
        Assert.Null(location.Photo.ContentUrl);
        Assert.Equal(asset.FileInfo.FileName, location.Photo.FileName);
    }

    [Fact]
    public async Task GetLocations_WithReadyPhoto_ReturnsAvailablePhotoWithTemporaryUrlAndLocalMetadata()
    {
        // Arrange
        var locationId = await CreateLocation();
        var asset = CreateAsset(status: "ready", assetType: "preview");
        FileService.Add(asset);
        await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, asset.Id), CancellationToken.None));

        // Act
        var result = await Handle<IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>, PagedResult<LocationDto>>(
            handler => handler.HandleAsync(new GetLocationsQuery(null, null), CancellationToken.None));

        // Assert
        var location = Assert.Single(result.Items);
        Assert.NotNull(location.Photo);
        Assert.Equal(LocationPhotoStatuses.Available, location.Photo.Status);
        Assert.Equal(asset.DownloadUrl, location.Photo.ContentUrl);
        Assert.Equal(asset.Id, location.Photo.AssetId);
        Assert.Equal(asset.FileInfo.FileName, location.Photo.FileName);
        Assert.Equal(asset.FileInfo.ContentType, location.Photo.ContentType);
        Assert.Equal(asset.FileInfo.Size, location.Photo.Size);
        Assert.NotEqual(default, location.Photo.VerifiedAt);
    }

    [Theory]
    [InlineData("deleted")]
    [InlineData(null)]
    public async Task AttachPhoto_WithDeletedOrMissingAsset_IsRejected(string? assetStatus)
    {
        // Arrange
        var locationId = await CreateLocation();
        var assetId = Guid.NewGuid();
        if (assetStatus is not null)
        {
            FileService.Add(CreateAsset(
                status: assetStatus,
                assetType: "preview",
                id: assetId));
        }

        // Act
        var result = await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, assetId), CancellationToken.None));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "media.asset.not.found");
        Assert.Null(await GetPhoto(locationId));
    }

    [Fact]
    public async Task AttachPhoto_WhenFileServiceUnavailable_IsRejectedWithoutPersistingPhoto()
    {
        // Arrange
        var locationId = await CreateLocation();
        FileService.IsUnavailable = true;

        // Act
        var result = await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, Guid.NewGuid()), CancellationToken.None));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "file-service.unavailable");
        Assert.Null(await GetPhoto(locationId));
    }

    [Fact]
    public async Task GetLocations_WhenAttachedAssetDisappears_ReturnsMissingPhotoWithLocalMetadata()
    {
        // Arrange
        var locationId = await CreateLocation();
        var asset = CreateAsset(status: "ready", assetType: "preview");
        FileService.Add(asset);
        await Handle<ICommandHandler<Guid, AttachPhotoRequest>, Result<Guid, Errors>>(handler =>
            handler.HandleAsync(new AttachPhotoRequest(locationId, asset.Id), CancellationToken.None));
        FileService.Remove(asset.Id);

        // Act
        var result = await Handle<IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>, PagedResult<LocationDto>>(
            handler => handler.HandleAsync(new GetLocationsQuery(null, null), CancellationToken.None));

        // Assert
        var location = Assert.Single(result.Items);
        Assert.NotNull(location.Photo);
        Assert.Equal(LocationPhotoStatuses.Missing, location.Photo.Status);
        Assert.Null(location.Photo.ContentUrl);
        Assert.Equal(asset.Id, location.Photo.AssetId);
        Assert.Equal(asset.FileInfo.FileName, location.Photo.FileName);
        Assert.Equal(asset.FileInfo.ContentType, location.Photo.ContentType);
        Assert.Equal(asset.FileInfo.Size, location.Photo.Size);
    }

    private async Task<Guid> CreateLocation()
    {
        var location = Location.Create(
            Name.Create($"Location {Guid.NewGuid()}").Value,
            Address.Create("Moscow", "Tverskaya", Guid.NewGuid().ToString(), null).Value,
            Timezone.Create("UTC").Value).Value;

        await ExecuteInDb(async db =>
        {
            db.Locations.Add(location);
            await db.SaveChangesAsync();
        });

        return location.Id;
    }

    private static MediaAssetInfoResponse CreateAsset(
        string status,
        string assetType,
        string contentType = "image/jpeg",
        Guid? id = null) => new(
        id ?? Guid.NewGuid(),
        status,
        assetType,
        DateTime.UtcNow,
        DateTime.UtcNow,
        new FileInfoDto("location.jpg", contentType, 1024),
        "https://files.test/location.jpg");

    private Task<LocationPhoto?> GetPhoto(Guid locationId) => ExecuteInDb(db => db.Locations
        .AsNoTracking()
        .Where(location => location.Id == locationId)
        .Select(location => location.Photo)
        .SingleAsync());

    private async Task<TResult> Handle<THandler, TResult>(Func<THandler, Task<TResult>> action)
        where THandler : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<THandler>());
    }
}
