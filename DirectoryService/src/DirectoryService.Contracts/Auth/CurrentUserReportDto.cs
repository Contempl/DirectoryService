namespace DirectoryService.Contracts.Auth;

public sealed record CurrentUserReportDto(
    Guid UserId,
    string Email,
    string Name,
    IReadOnlyList<string> Roles,
    IReadOnlySet<string> DirectoryPermissions,
    IReadOnlySet<string> AuthPermissions,
    bool EmailConfirmed,
    DateTime AccountCreatedAt);
