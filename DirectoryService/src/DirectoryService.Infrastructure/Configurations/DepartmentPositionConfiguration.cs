using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(dp => dp.Id);
        
        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();
        
        builder.Property(dl => dl.PositionId)
            .HasColumnName("position_id");
    }
}