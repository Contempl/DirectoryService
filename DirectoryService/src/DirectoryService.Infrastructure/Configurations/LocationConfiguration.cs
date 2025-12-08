using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");
        
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Address)
            .HasColumnName("address")
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");
        
        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.ComplexProperty(l => l.Timezone, lt =>
        {
            lt.Property(t => t.Value)
                .IsRequired()
                .HasColumnName("timezone");
        });
        
        builder.ComplexProperty(l => l.Name, ln =>
        {
            ln.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("name");
        });
        
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");
    }
}