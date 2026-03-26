using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Core.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.ToTable("RefreshTokens");
        
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.Token)
            .HasMaxLength(200)
            .HasColumnName("token");
        
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(r => r.ExpiryDate)
            .HasColumnName("expiry_date");
        
        builder.Property(r => r.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(r => r.IsRevoked)
            .HasColumnName("is_revoked");

        builder.Property(r => r.JwtId)
            .HasMaxLength(150)
            .HasColumnName("jwt_id");

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(200)
            .HasColumnName("replacing_token");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id");
        
        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}