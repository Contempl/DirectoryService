using System.Net;
using System.Net.Http.Json;
using AuthService.Application.Features.Login;
using AuthService.Application.Features.Register;

namespace AuthService.IntegrationTests.Login;

public class LoginTests : AuthServiceBaseTests
{
    private const string Email = "loginuser@test.com";
    private const string Password = "Test@Pass123!";

    public LoginTests(AuthServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithConfirmedEmail_ShouldReturnTokens()
    {
        await RegisterAndConfirmAsync(Email, Password);

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrEmpty(loginResponse.AccessToken));
        Assert.False(string.IsNullOrEmpty(loginResponse.RefreshToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnError()
    {
        await RegisterAndConfirmAsync(Email, Password);

        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(Email, "WrongPass!99"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ShouldReturnError()
    {
        var request = new RegisterRequest(Email, Password, "Test", "User");
        await Client.PostAsJsonAsync("/api/auth/register", request);

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterMaxFailedAttempts_ShouldLockAccount()
    {
        await RegisterAndConfirmAsync(Email, Password);

        for (var i = 0; i < 5; i++)
        {
            await Client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(Email, "WrongPass!99"));
        }

        // Correct password but account is locked
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(Email, Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
