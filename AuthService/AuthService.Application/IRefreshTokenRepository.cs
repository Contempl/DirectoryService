using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IRefreshTokensRepository
{
   Task<Result<RefreshToken, Error>> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

   Task<UnitResult<Error>> RevokeAllRefreshTokensFromUser(Guid userId,  CancellationToken cancellationToken = default);
   
   Task<UnitResult<Error>> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
   
   Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);
}