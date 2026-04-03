using AuthService.Application;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Core.Repositories;

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly AuthDbContext _authDbContext;
    private readonly ILogger<RefreshTokensRepository> _logger;

    public RefreshTokensRepository(AuthDbContext authDbContext, ILogger<RefreshTokensRepository> logger)
    {
        _authDbContext = authDbContext;
        _logger = logger;
    }

    public async Task<Result<RefreshToken, Error>> GetByTokenAsync(string token, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _authDbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        
        if (refreshToken is null)
        {
            _logger.LogInformation("RefreshToken not found");
            return GeneralErrors.NotFound(name: nameof(RefreshToken));
        }
        
        return refreshToken;
    }


    public async Task<UnitResult<Error>> RevokeAllRefreshTokensFromUser(Guid userId)
    {
        var tokens = await _authDbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.Revoke();

        await _authDbContext.SaveChangesAsync();

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _authDbContext.AddAsync(refreshToken, cancellationToken);
        try
        {
            await _authDbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
            return Error.Failure("database", "Failed to save changes");
        }
    }
}