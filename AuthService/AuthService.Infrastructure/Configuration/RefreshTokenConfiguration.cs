using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Core.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.ToTable("refresh_tokens");
        
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.TokenHash)
            .HasMaxLength(64)
            .HasColumnName("token_hash")
            .IsRequired();
        
        builder.HasIndex(r => r.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash");
        
        builder.Property(r => r.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();
        
        builder.HasIndex(r => r.FamilyId)
            .HasDatabaseName("ix_refresh_tokens_family_id");
        
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

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(64)
            .HasColumnName("replaced_by_token_hash");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id");
        
        builder.Property<uint>("xmin")
            .IsRowVersion();
        
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}