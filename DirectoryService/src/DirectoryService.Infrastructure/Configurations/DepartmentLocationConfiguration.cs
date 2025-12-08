using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(dl => dl.Id)
            .HasName("id");

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");
        
        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id");
    }
}