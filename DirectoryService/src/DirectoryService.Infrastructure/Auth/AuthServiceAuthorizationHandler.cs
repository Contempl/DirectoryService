using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace DirectoryService.Infrastructure.Auth;

internal sealed class AuthServiceAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IServiceTokenProvider serviceTokenProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var incomingAuthorization = httpContext?
            .Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(incomingAuthorization))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                incomingAuthorization);
        }
        else if (httpContext is null)
        {
            var serviceToken = await serviceTokenProvider
                .GetTokenAsync(cancellationToken);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    serviceToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
