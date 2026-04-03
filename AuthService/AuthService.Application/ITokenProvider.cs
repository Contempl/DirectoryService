using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface ITokenProvider
{
    string GenerateJwtToken(ApplicationUser user, List<string> roles, HashSet<string> permissions);
    Result<RefreshToken, Error> GenerateRefreshToken(Guid userId, string jwtTokenId);
}