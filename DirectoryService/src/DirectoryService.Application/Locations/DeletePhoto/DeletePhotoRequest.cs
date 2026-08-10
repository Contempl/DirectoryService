using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.DeletePhoto;

public record DeletePhotoRequest(Guid LocationId) : ICommand;