using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Features.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.IntegrationTests.Register;

public class RegisterTests : AuthServiceBaseTests
{
    public RegisterTests(AuthServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnUserIdAndCreateUser()
    {
        var request = new RegisterRequest("newuser@test.com", "Test@Pass123!", "John", "Doe");

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var userId = json.GetProperty("result").GetGuid();
        Assert.NotEqual(Guid.Empty, userId);

        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Domain.Entities.ApplicationUser>>();
        var user = await userManager.FindByEmailAsync("newuser@test.com");
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        var request = new RegisterRequest("dup@test.com", "Test@Pass123!", "John", "Doe");

        await Client.PostAsJsonAsync("/api/auth/register", request);
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        var request = new RegisterRequest("user@test.com", "weakpassword", "John", "Doe");

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
