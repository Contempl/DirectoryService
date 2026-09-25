using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    
    public string JwtId { get; init; } = string.Empty;     // идентификатор связанного JWT (опционально)
    
    public DateTime ExpiryDate { get; init; }              // когда истекает
    
    public bool IsRevoked { get; private set; }            // отозван ли
    
    public DateTime? RevokedAt { get; private set; }
    
    public DateTime CreatedAt { get; init; }
    
    public Guid UserId { get; init; }
    
    public string TokenHash { get; private init; } = string.Empty;
    
    public Guid FamilyId { get; init; }
    
    public string? ReplacedByTokenHash { get; private set; }
    
    private RefreshToken() { }

    private RefreshToken(string tokenHash, Guid userId, string jwtId, DateTime expiryDate, Guid familyId)
    {
        Id = Guid.CreateVersion7();
        TokenHash = tokenHash;
        UserId = userId;
        JwtId = jwtId;
        ExpiryDate = expiryDate;
        CreatedAt = DateTime.UtcNow;
        FamilyId = familyId;
    }

    private static Result<RefreshToken, Error> Create(
        string tokenHash,
        Guid userId,
        string jwtTokenId,
        DateTime expiryDate,
        Guid familyId)
    {
        return new RefreshToken(tokenHash, userId, jwtTokenId, expiryDate, familyId);
    }

    public UnitResult<Error> Revoke(string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        
        return UnitResult.Success<Error>();
    }
    
    public static Result<RefreshToken, Error> CreateInitial(
        string tokenHash,
        Guid userId,
        string jwtId,
        DateTime expiryDate)
    {
        return Create(
            tokenHash,
            userId,
            jwtId,
            expiryDate,
            Guid.CreateVersion7());
    }

    public static Result<RefreshToken, Error> CreateRotated(
        string tokenHash,
        Guid userId,
        string jwtId,
        DateTime expiryDate,
        Guid familyId)
    {
        return Create(
            tokenHash,
            userId,
            jwtId,
            expiryDate,
            familyId);
    }
}
