using System.Net;
using System.Net.Http.Headers;
using Core.Auth;

namespace DirectoryService.IntegrationTests.Authorization;

public sealed class JwtAuthorizationTests(
    DirectoryServiceAuthorizationTestWebFactory factory)
    : IClassFixture<DirectoryServiceAuthorizationTestWebFactory>
{
    [Fact]
    public async Task Locations_AcceptsTrustedSignature_AndRejectsForeignSignature()
    {
        using var client = factory.CreateClient();

        using var trustedRequest = new HttpRequestMessage(
            HttpMethod.Get, "/api/locations");
        trustedRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateTestAccessToken(SystemRoles.User));
        using var trustedResponse = await client.SendAsync(trustedRequest);
        Assert.Equal(HttpStatusCode.OK, trustedResponse.StatusCode);

        using var forgedRequest = new HttpRequestMessage(
            HttpMethod.Get, "/api/locations");
        forgedRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateTestAccessToken(
                SystemRoles.User, foreignSignature: true));
        using var forgedResponse = await client.SendAsync(forgedRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, forgedResponse.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_WithoutTokenOrPermission_IsDenied()
    {
        using var client = factory.CreateClient();
        var path = "/api/locations";

        using var anonymousResponse = await client.PostAsync(path, null);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var userRequest = new HttpRequestMessage(HttpMethod.Post, path);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateTestAccessToken(SystemRoles.User));
        using var userResponse = await client.SendAsync(userRequest);
        Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
    }
}
