using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");
        
        builder.ComplexProperty(d => d.Name, nb =>
        {
            nb.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("name");
        });
        
        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.ComplexProperty(d => d.Identifier, di =>
        {
            di.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("identifier");
        });

        builder.ComplexProperty(d => d.Path, dp =>
        {
            dp.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("path");
        });

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");
        
        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}