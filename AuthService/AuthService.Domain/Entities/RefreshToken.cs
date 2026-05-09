using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    
    public string Token { get; init; } = string.Empty;     // сам refresh token (строка)
    
    public string JwtId { get; init; } = string.Empty;     // идентификатор связанного JWT (опционально)
    
    public DateTime ExpiryDate { get; init; }              // когда истекает
    
    public bool IsRevoked { get; private set; }            // отозван ли
    
    public DateTime? RevokedAt { get; private set; }
    
    public string? ReplacedByToken { get; private set; }   // если был заменён новым токеном
    
    public DateTime CreatedAt { get; init; }
    
    public Guid UserId { get; init; }
    
    private RefreshToken() { }

    private RefreshToken(string token, Guid userId, string jwtId, DateTime expiryDate)
    {
        Id = Guid.NewGuid();
        Token = token;
        UserId = userId;
        JwtId = jwtId;
        ExpiryDate = expiryDate;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<RefreshToken, Error> Create(
        string token,
        Guid userId,
        string jwtTokenId,
        DateTime expiryDate)
    {
        try
        {
            var jwtId =  jwtTokenId;
        
            var refreshToken = new RefreshToken(token, userId, jwtId, expiryDate);
        
            return refreshToken;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid("failed to create refresh token");
        }
    }

    public UnitResult<Error> Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
        
        return UnitResult.Success<Error>();
    }
}