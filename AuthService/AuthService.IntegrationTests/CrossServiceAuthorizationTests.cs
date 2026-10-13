using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DirectoryService.IntegrationTests.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthService.IntegrationTests;

public sealed class CrossServiceAuthorizationTests(
    AuthServiceTestWebFactory authFactory)
    : IClassFixture<AuthServiceTestWebFactory>
{
    [Fact]
    public async Task DirectoryService_AcceptsAuthServiceToken_AndRejectsForeignSignature()
    {
        using var authClient = authFactory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("http://localhost")
            });

        var accessToken = await GetAdminAccessTokenAsync(authClient);
        var signingKeyId = new JwtSecurityTokenHandler()
            .ReadJwtToken(accessToken).Header.Kid;
        Assert.False(string.IsNullOrWhiteSpace(signingKeyId));

        // The resource server fetches discovery and JWKS from the actual
        // AuthService TestServer, without exposing a network port.
        using var identityProviderHandler = authFactory.Server.CreateHandler();
        var directoryFactory =
            DirectoryServiceAuthorizationTestWebFactory.WithIdentityProvider(
                identityProviderHandler);

        try
        {
            await directoryFactory.InitializeAsync();
            using var directoryClient = directoryFactory.CreateClient();

            using var trustedRequest = new HttpRequestMessage(
                HttpMethod.Get, "/api/locations");
            trustedRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
            using var trustedResponse = await directoryClient.SendAsync(
                trustedRequest);
            Assert.Equal(HttpStatusCode.OK, trustedResponse.StatusCode);

            using var forgedRequest = new HttpRequestMessage(
                HttpMethod.Get, "/api/locations");
            forgedRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", directoryFactory.CreateTestAccessToken(
                    "Admin",
                    foreignSignature: true,
                    signingKeyId: signingKeyId));
            using var forgedResponse = await directoryClient.SendAsync(
                forgedRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, forgedResponse.StatusCode);
        }
        finally
        {
            await directoryFactory.DisposeAsync();
        }
    }

    private static async Task<string> GetAdminAccessTokenAsync(HttpClient client)
    {
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = AuthServiceTestWebFactory.AdminEmail,
                Password = AuthServiceTestWebFactory.AdminPassword
            });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var verifier = Base64Url(RandomNumberGenerator.GetBytes(64));
        var challenge = Base64Url(SHA256.HashData(
            Encoding.ASCII.GetBytes(verifier)));
        var authorizeUri = QueryHelpers.AddQueryString(
            "/connect/authorize",
            new Dictionary<string, string?>
            {
                ["client_id"] = AuthServiceTestWebFactory.FrontendClientId,
                ["redirect_uri"] = AuthServiceTestWebFactory.RedirectUri,
                ["response_type"] = "code",
                ["scope"] =
                    "openid profile email roles offline_access directory_api",
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256",
                ["state"] = Guid.NewGuid().ToString("N")
            });

        using var authorizeResponse = await client.GetAsync(authorizeUri);
        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);
        Assert.NotNull(authorizeResponse.Headers.Location);
        var code = QueryHelpers.ParseQuery(
            authorizeResponse.Headers.Location.Query)["code"].ToString();
        Assert.False(string.IsNullOrWhiteSpace(code));

        using var tokenResponse = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
            [
                new("grant_type", "authorization_code"),
                new("client_id", AuthServiceTestWebFactory.FrontendClientId),
                new("client_secret", AuthServiceTestWebFactory.FrontendClientSecret),
                new("redirect_uri", AuthServiceTestWebFactory.RedirectUri),
                new("code", code),
                new("code_verifier", verifier)
            ]));
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        using var document = JsonDocument.Parse(
            await tokenResponse.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("access_token").GetString()!;
    }

    private static string Base64Url(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
