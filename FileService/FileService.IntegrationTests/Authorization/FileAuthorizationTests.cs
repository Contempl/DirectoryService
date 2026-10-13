using System.Net;
using System.Net.Http.Headers;
using Core.Auth;
using FileService.IntegrationTests.Infrastructure;

namespace FileService.IntegrationTests.Authorization;

[Collection(FileServiceTestCollection.Name)]
public sealed class FileAuthorizationTests(FileServiceTestWebFactory factory)
{
    [Fact]
    public async Task DeleteFile_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFile_WithUserRole_ReturnsForbidden()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateTestAccessToken(SystemRoles.User));

        using var response = await client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
