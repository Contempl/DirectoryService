using AuthService.Application;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Shared.Kernel;

namespace AuthService.Core.Identity;

public sealed class OpenIddictAuthorizationSessionRevoker(
    IOpenIddictTokenManager tokenManager,
    ILogger<OpenIddictAuthorizationSessionRevoker> logger)
    : IAuthorizationSessionRevoker
{
    public async Task<UnitResult<Error>> RevokeAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await tokenManager.RevokeBySubjectAsync(
                userId.ToString(),
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to revoke OpenIddict tokens for user {UserId}.",
                userId);

            return GeneralErrors.Failure();
        }
    }
}
