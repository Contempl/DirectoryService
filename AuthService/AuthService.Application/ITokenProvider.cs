using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface ITokenProvider
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    Result<RefreshToken, Error> GenerateRefreshToken(Guid userId, Guid jwtTokenId);
}