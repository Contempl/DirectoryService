namespace DirectoryService.Application.Auth;

public sealed record AuthUserProfile(
    IReadOnlyList<string> Roles,
    IReadOnlySet<string> AuthPermissions,
    bool EmailConfirmed,
    DateTime CreatedAt);
