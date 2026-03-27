using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Core.Configuration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.ToTable("application_users");

        builder.Property(u => u.Id)
            .IsRequired()
            .HasColumnName("id");
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");
        
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasColumnName("is_active");
        
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(u => u.Id)
            .IsUnique()
            .HasDatabaseName("ix_users_id");

        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("ix_users_names_descending")
            .IsDescending();
    }
}