using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.ReplacePhoto;

public record ReplacePhotoRequest(Guid LocationId, Guid AssetId) : ICommand;