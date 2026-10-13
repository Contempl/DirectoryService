using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IAuthorizationSessionRevoker
{
    Task<UnitResult<Error>> RevokeAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
