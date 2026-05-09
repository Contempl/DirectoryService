namespace AuthService.Contracts.Dto;

public record UserProfileDto(
    IReadOnlyList<string> Roles, 
    IReadOnlySet<string> Permissions,
    bool EmailConfirmed,
    DateTime CreatedAt);