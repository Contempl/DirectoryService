namespace DirectoryService.Application.Auth;

public sealed record AuthUserSummary(
    Guid Id,
    IReadOnlyList<string> Roles,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive);
