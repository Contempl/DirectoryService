namespace DirectoryService.Presentation.Authorization;

public sealed class IdentityProviderOptions
{
    public const string SectionName = "IdentityProvider";

    public string Authority { get; init; } = string.Empty;
    public string MetadataAddress { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public bool RequireHttpsMetadata { get; init; } = true;
}
