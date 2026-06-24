using System.Net;
using System.Net.Http.Headers;

namespace AuthService.IntegrationTests.ProtectedEndpoints;

public class ProtectedEndpointsTests : AuthServiceBaseTests
{
    private const string Email = "protecteduser@test.com";
    private const string Password = "Test@Pass123!";

    public ProtectedEndpointsTests(AuthServiceTestWebFactory factory) : base(factory) { }

    public override async Task InitializeAsync() =>
        await Factory.CreateAdminAsync();

    [Fact]
    public async Task GetUsers_WithoutToken_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithInvalidToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this.is.not.a.valid.token");

        var response = await Client.GetAsync("/api/users");

        Client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithValidTokenNoPermission_ShouldReturn403()
    {
        await RegisterAndConfirmAsync(Email, Password);
        var tokens = await LoginAsync(Email, Password);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await Client.GetAsync("/api/users?Page=1&PageSize=10");

        Client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithAdminToken_ShouldReturn200()
    {
        var tokens = await LoginAsync(AuthServiceTestWebFactory.TestAdminEmail,
            AuthServiceTestWebFactory.TestAdminPassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await Client.GetAsync("/api/users?Page=1&PageSize=10");

        Client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
