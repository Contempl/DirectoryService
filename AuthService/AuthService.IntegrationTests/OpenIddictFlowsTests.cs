using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.IntegrationTests;

public sealed class OpenIddictFlowsTests(
    AuthServiceTestWebFactory factory)
    : IClassFixture<AuthServiceTestWebFactory>
{
    [Fact]
    public async Task DiscoveryAndJwks_ArePublished()
    {
        using var client = CreateClient(handleCookies: false);

        using var discoveryResponse = await client.GetAsync(
            "/.well-known/openid-configuration");
        Assert.Equal(HttpStatusCode.OK, discoveryResponse.StatusCode);

        using var discovery = JsonDocument.Parse(
            await discoveryResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "http://localhost/connect/token",
            discovery.RootElement.GetProperty("token_endpoint").GetString());

        using var jwksResponse = await client.GetAsync(
            "/.well-known/jwks");
        Assert.Equal(HttpStatusCode.OK, jwksResponse.StatusCode);

        using var jwks = JsonDocument.Parse(
            await jwksResponse.Content.ReadAsStringAsync());
        Assert.NotEmpty(jwks.RootElement.GetProperty("keys").EnumerateArray());
    }

    [Fact]
    public async Task Authorize_WithoutProviderSession_RedirectsToLogin()
    {
        using var client = CreateClient(handleCookies: false);

        using var response = await client.GetAsync(
            CreateAuthorizeUri(CreatePkce().Challenge));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith(
            "http://client.test/login",
            response.Headers.Location!.ToString());

        var query = QueryHelpers.ParseQuery(response.Headers.Location.Query);
        Assert.StartsWith("/connect/authorize", query["returnUrl"].ToString());
    }

    [Fact]
    public async Task Authorize_AfterSecurityStampChanges_RequiresLoginAgain()
    {
        using var client = CreateClient(handleCookies: true);
        await LoginAsync(client);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(
                AuthServiceTestWebFactory.AdminEmail);
            Assert.NotNull(user);
            var updateResult = await userManager.UpdateSecurityStampAsync(user);
            Assert.True(updateResult.Succeeded);
        }

        using var response = await client.GetAsync(
            CreateAuthorizeUri(CreatePkce().Challenge));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith(
            "http://client.test/login",
            response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task AuthorizationCodeRefreshAndUserInfo_WorkEndToEnd()
    {
        using var client = CreateClient(handleCookies: true);
        await LoginAsync(client);

        var pkce = CreatePkce();
        var code = await GetAuthorizationCodeAsync(client, pkce.Challenge);
        var tokens = await ExchangeCodeAsync(client, code, pkce.Verifier);

        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.IdentityToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));

        using var anonymousUserInfo = await client.GetAsync(
            "/connect/userinfo");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousUserInfo.StatusCode);

        using var userInfoRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/connect/userinfo");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokens.AccessToken);
        using var userInfoResponse = await client.SendAsync(userInfoRequest);
        Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);

        using var userInfo = JsonDocument.Parse(
            await userInfoResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            AuthServiceTestWebFactory.AdminEmail,
            userInfo.RootElement.GetProperty("email").GetString());

        using var refreshResponse = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
            [
                new("grant_type", "refresh_token"),
                new("client_id", AuthServiceTestWebFactory.FrontendClientId),
                new("client_secret", AuthServiceTestWebFactory.FrontendClientSecret),
                new("refresh_token", tokens.RefreshToken!),
                new("scope", "openid profile email roles offline_access directory_api")
            ]));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = await ReadTokenResponseAsync(refreshResponse);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
    }

    [Fact]
    public async Task AuthorizationCode_WithoutVerifier_IsRejected()
    {
        using var client = CreateClient(handleCookies: true);
        await LoginAsync(client);

        var pkce = CreatePkce();
        var code = await GetAuthorizationCodeAsync(client, pkce.Challenge);

        using var response = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
            [
                new("grant_type", "authorization_code"),
                new("client_id", AuthServiceTestWebFactory.FrontendClientId),
                new("client_secret", AuthServiceTestWebFactory.FrontendClientSecret),
                new("redirect_uri", AuthServiceTestWebFactory.RedirectUri),
                new("code", code)
            ]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ClientCredentials_IssuesServiceAccessToken()
    {
        using var client = CreateClient(handleCookies: false);

        using var response = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
            [
                new("grant_type", "client_credentials"),
                new("client_id", AuthServiceTestWebFactory.ServiceClientId),
                new("client_secret", AuthServiceTestWebFactory.ServiceClientSecret),
                new("scope", AuthServiceTestWebFactory.ApiScope)
            ]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await ReadTokenResponseAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));

        using var usersRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/users?Page=1&PageSize=10");
        usersRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token.AccessToken);
        using var usersResponse = await client.SendAsync(usersRequest);
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
    }

    [Fact]
    public async Task PublishedJwks_AcceptsRealToken_AndRejectsForeignSignature()
    {
        using var client = CreateClient(handleCookies: false);
        var accessToken = await GetServiceAccessTokenAsync(client);

        using var discoveryResponse = await client.GetAsync(
            "/.well-known/openid-configuration");
        using var discovery = JsonDocument.Parse(
            await discoveryResponse.Content.ReadAsStringAsync());
        var issuer = discovery.RootElement.GetProperty("issuer").GetString()!;
        var jwksUri = discovery.RootElement.GetProperty("jwks_uri").GetString()!;

        using var jwksResponse = await client.GetAsync(jwksUri);
        var keySet = new JsonWebKeySet(
            await jwksResponse.Content.ReadAsStringAsync());
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = AuthServiceTestWebFactory.ApiScope,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keySet.GetSigningKeys(),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role"
        };

        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        var principal = handler.ValidateToken(
            accessToken,
            validationParameters,
            out _);
        Assert.Contains(
            principal.Claims,
            claim => claim.Type == "role" && claim.Value == "ServiceAccount");

        using var foreignKey = RSA.Create(2048);
        var forged = handler.WriteToken(new JwtSecurityToken(
            issuer,
            AuthServiceTestWebFactory.ApiScope,
            [new Claim("sub", Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new RsaSecurityKey(foreignKey),
                SecurityAlgorithms.RsaSha256)));

        Assert.ThrowsAny<SecurityTokenException>(() =>
            handler.ValidateToken(forged, validationParameters, out _));
    }

    private HttpClient CreateClient(bool handleCookies)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = handleCookies,
            BaseAddress = new Uri("http://localhost")
        });
    }

    private static async Task LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = AuthServiceTestWebFactory.AdminEmail,
                Password = AuthServiceTestWebFactory.AdminPassword
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<string> GetAuthorizationCodeAsync(
        HttpClient client,
        string challenge)
    {
        using var response = await client.GetAsync(CreateAuthorizeUri(challenge));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith(
            AuthServiceTestWebFactory.RedirectUri,
            response.Headers.Location!.ToString());

        var query = QueryHelpers.ParseQuery(response.Headers.Location.Query);
        return query["code"].ToString();
    }

    private static async Task<TokenResponse> ExchangeCodeAsync(
        HttpClient client,
        string code,
        string verifier)
    {
        using var response = await client.PostAsync(
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadTokenResponseAsync(response);
    }

    private static async Task<string> GetServiceAccessTokenAsync(
        HttpClient client)
    {
        using var response = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
            [
                new("grant_type", "client_credentials"),
                new("client_id", AuthServiceTestWebFactory.ServiceClientId),
                new("client_secret", AuthServiceTestWebFactory.ServiceClientSecret),
                new("scope", AuthServiceTestWebFactory.ApiScope)
            ]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await ReadTokenResponseAsync(response)).AccessToken;
    }

    private static string CreateAuthorizeUri(string challenge)
    {
        return QueryHelpers.AddQueryString(
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
    }

    private static (string Verifier, string Challenge) CreatePkce()
    {
        var verifier = Base64Url(RandomNumberGenerator.GetBytes(64));
        var challenge = Base64Url(
            SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        return (verifier, challenge);
    }

    private static string Base64Url(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static async Task<TokenResponse> ReadTokenResponseAsync(
        HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        return new TokenResponse(
            root.GetProperty("access_token").GetString()!,
            root.TryGetProperty("id_token", out var identityToken)
                ? identityToken.GetString()
                : null,
            root.TryGetProperty("refresh_token", out var refreshToken)
                ? refreshToken.GetString()
                : null);
    }

    private sealed record TokenResponse(
        string AccessToken,
        string? IdentityToken,
        string? RefreshToken);
}
