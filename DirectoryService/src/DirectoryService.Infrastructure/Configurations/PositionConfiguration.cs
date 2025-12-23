using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");
        
        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired(false);

        builder.ComplexProperty(p => p.Name, pn =>
        {
            pn.Property(n => n.Value)
                .HasMaxLength(100)
                .HasColumnType("name")
                .IsRequired();
        });
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");
        
    }
}