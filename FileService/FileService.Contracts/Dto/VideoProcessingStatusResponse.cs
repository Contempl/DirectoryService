namespace FileService.Contracts.Dto;

public sealed record VideoProcessingStatusResponse(
    Guid VideoAssetId,
    string Status,
    string? CurrentStep,
    int ProgressPercentage,
    string? ErrorMessage,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    bool IsTerminal);
