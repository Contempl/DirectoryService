using System.Text.Json;
using DirectoryService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Auth;

internal sealed class ServiceTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<ServiceTokenOptions> serviceTokenOptions,
    TimeProvider timeProvider)
    : IServiceTokenProvider
{
    private readonly ServiceTokenOptions _serviceTokenOptions =
        serviceTokenOptions.Value;

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _expiresAt;

    public async ValueTask<string> GetTokenAsync(
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (IsTokenUsable(now))
        {
            return _cachedToken!;
        }

        await _refreshLock.WaitAsync(cancellationToken);

        try
        {
            now = timeProvider.GetUtcNow();

            if (IsTokenUsable(now))
            {
                return _cachedToken!;
            }

            var token = await RequestTokenAsync(cancellationToken);
            _expiresAt = now.AddSeconds(token.ExpiresIn);
            _cachedToken = token.AccessToken;

            return _cachedToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool IsTokenUsable(DateTimeOffset now)
    {
        if (_cachedToken is null)
        {
            return false;
        }

        var refreshAt = _expiresAt.AddSeconds(
            -_serviceTokenOptions.RefreshBeforeExpirationSeconds);

        return now < refreshAt;
    }

    private async Task<TokenResponse> RequestTokenAsync(
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new("grant_type", "client_credentials"),
                new("client_id", _serviceTokenOptions.ClientId),
                new("client_secret", _serviceTokenOptions.ClientSecret),
                new("scope", _serviceTokenOptions.Scope)
            ])
        };

        var client = httpClientFactory.CreateClient(
            AuthServiceTokenClientName);
        using var response = await client.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<TokenResponse>(
            stream,
            cancellationToken: cancellationToken);

        return token is { AccessToken.Length: > 0, ExpiresIn: > 0 }
            ? token
            : throw new InvalidOperationException(
                "AuthService returned an invalid client credentials response.");
    }

    public const string AuthServiceTokenClientName =
        "AuthServiceTokenClient";

    private sealed record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")]
        string AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        int ExpiresIn);
}
