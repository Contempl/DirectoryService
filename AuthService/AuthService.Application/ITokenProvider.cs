using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface ITokenProvider
{
    public string GenerateJwtToken(ApplicationUser user, IReadOnlyCollection<string> roles);
    Result<GeneratedRefreshToken, Error> GenerateInitialRefreshToken(Guid userId, string jwtId);
    Result<GeneratedRefreshToken, Error> GenerateRotatedRefreshToken(Guid userId, string jwtId, Guid familyId);
    string HashRefreshToken(string rawToken);
}

public record GeneratedRefreshToken(
    string RawToken,
    RefreshToken Entity);