using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IRefreshTokensRepository
{
   Task<Result<RefreshToken, Error>> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

   Task<UnitResult<Error>> RevokeAllRefreshTokensFromUser(Guid userId);
   
   Task<UnitResult<Error>> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}