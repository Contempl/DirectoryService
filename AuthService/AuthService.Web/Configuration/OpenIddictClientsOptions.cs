namespace AuthService.Configuration;

public sealed class OpenIddictClientsOptions
{
    public const string SectionName = "OpenIddictClients";

    public OpenIddictClientOptions Frontend { get; init; } = new();
    public OpenIddictClientOptions DirectoryService { get; init; } = new();
}

public sealed class OpenIddictClientOptions
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? RedirectUri { get; init; }
    public Guid? SubjectId { get; init; }
}
