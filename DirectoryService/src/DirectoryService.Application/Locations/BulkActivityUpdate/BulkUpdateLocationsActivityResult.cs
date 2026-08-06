namespace DirectoryService.Application.Locations.BulkActivityUpdate;

public sealed record BulkLocationError(
    Guid LocationId,
    string Message);

public sealed record BulkUpdateLocationsActivityResult(
    int ProcessedCount,
    List<BulkLocationError> Errors);