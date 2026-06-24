using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Features.Login;
using AuthService.Application.Features.RefreshToken;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.IntegrationTests.RefreshToken;

public class RefreshTokenTests : AuthServiceBaseTests
{
    private const string Email = "refreshuser@test.com";
    private const string Password = "Test@Pass123!";

    public RefreshTokenTests(AuthServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturnNewTokens()
    {
        await RegisterAndConfirmAsync(Email, Password);
        var tokens = await LoginAsync(Email, Password);

        var request = new RefreshTokenRequest(tokens.AccessToken, tokens.RefreshToken);
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var newTokens = json.GetProperty("result").Deserialize<LoginResponse>();
        Assert.NotNull(newTokens);
        Assert.NotEqual(tokens.AccessToken, newTokens.AccessToken);
        Assert.NotEqual(tokens.RefreshToken, newTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldRevokeAllTokensAndFail()
    {
        await RegisterAndConfirmAsync(Email, Password);
        var tokens = await LoginAsync(Email, Password);

        // Use token once — original is revoked, new token issued
        var firstRefresh = new RefreshTokenRequest(tokens.AccessToken, tokens.RefreshToken);
        var firstResponse = await Client.PostAsJsonAsync("/api/auth/refresh", firstRefresh);
        firstResponse.EnsureSuccessStatusCode();

        // Reuse the now-revoked original token — triggers security response
        var secondResponse = await Client.PostAsJsonAsync("/api/auth/refresh", firstRefresh);

        Assert.Equal(HttpStatusCode.InternalServerError, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ShouldFail()
    {
        await RegisterAndConfirmAsync(Email, Password);
        var tokens = await LoginAsync(Email, Password);

        // Expire the refresh token directly in DB
        await ExecuteInDb(async db =>
        {
            await db.RefreshTokens
                .Where(t => t.Token == tokens.RefreshToken)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    t => t.ExpiryDate, DateTime.UtcNow.AddDays(-1)));
        });

        var request = new RefreshTokenRequest(tokens.AccessToken, tokens.RefreshToken);
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
