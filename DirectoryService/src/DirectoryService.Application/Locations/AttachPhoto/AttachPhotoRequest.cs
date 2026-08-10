using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.AttachPhoto;

public record AttachPhotoRequest(Guid LocationId, Guid AssetId) : ICommand;