namespace AuthService.Core.Options;

public record AdminOptions
{
    public string Username { get; init; } = string.Empty;
    
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;
    
    public string Email { get; init; } = string.Empty;
    
    public string Password { get; init; } = string.Empty;
}