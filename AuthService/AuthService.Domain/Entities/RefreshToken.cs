namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    
    public string Token { get; init; } = string.Empty;     // сам refresh token (строка)
    
    public string JwtId { get; init; } = string.Empty;     // идентификатор связанного JWT (опционально)
    
    public DateTime ExpiryDate { get; init; }              // когда истекает
    
    public bool IsRevoked { get; private set; }            // отозван ли
    
    public DateTime? RevokedAt { get; init; }
    
    public string? ReplacedByToken { get; private set; }   // если был заменён новым токеном
    
    public DateTime CreatedAt { get; init; }
    
    public Guid UserId { get; init; }
    
    public ApplicationUser User { get; init; } = null!;

    private RefreshToken() { }
}