using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.BulkActivityUpdate;

public sealed record BulkUpdateLocationsActivityRequest(
    List<Guid> LocationIds,
    bool IsActive) : ICommand;