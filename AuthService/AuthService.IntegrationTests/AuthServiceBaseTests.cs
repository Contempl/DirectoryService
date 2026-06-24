using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Features.Login;
using AuthService.Application.Features.Register;
using AuthService.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.IntegrationTests;

public abstract class AuthServiceBaseTests : IClassFixture<AuthServiceTestWebFactory>, IAsyncLifetime
{
    protected readonly AuthServiceTestWebFactory Factory;
    protected readonly HttpClient Client;

    protected AuthServiceBaseTests(AuthServiceTestWebFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<T> ExecuteInDb<T>(Func<AuthDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<AuthDbContext, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await action(dbContext);
    }

    protected async Task<Guid> RegisterAndConfirmAsync(string email, string password = "Test@Pass123!")
    {
        var registerRequest = new RegisterRequest(email, password, "Test", "User");
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var json = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = json.GetProperty("result").GetGuid();

        var token = Factory.FakeEmailSender.GetConfirmationToken(email)!;
        var confirmUrl = QueryHelpers.AddQueryString("/api/auth/confirm-email",
            new Dictionary<string, string?> { ["userId"] = userId.ToString(), ["token"] = token });

        var confirmResponse = await Client.GetAsync(confirmUrl);
        confirmResponse.EnsureSuccessStatusCode();

        return userId;
    }

    protected async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await Factory.ResetDatabaseAsync();
}
