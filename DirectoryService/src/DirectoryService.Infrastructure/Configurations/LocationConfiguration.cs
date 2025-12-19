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

        builder.OwnsOne(l => l.Address, la =>
        {
            la.Property(a => a.City)
                .HasColumnName("city");
            
            la.Property(a => a.Street)
                .HasColumnName("street");
            
            la.Property(a => a.House)
                .HasColumnName("house");
            
            la.Property(a => a.Apartment)
                .HasColumnName("apartment");
            
            la.HasIndex(x => new { x.City, x.Street, x.House, x.Apartment })
                .IsUnique()
                .HasDatabaseName("ix_locations_address");
        });
        
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
        
        builder.OwnsOne(l => l.Name, ln =>
        {
            ln.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("name");
            
            ln.HasIndex(x => x.Value)
                .IsUnique()
                .HasDatabaseName("ix_locations_name");
        });
        
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");
    }
}