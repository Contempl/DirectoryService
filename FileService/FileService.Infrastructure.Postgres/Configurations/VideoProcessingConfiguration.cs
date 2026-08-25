using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class VideoProcessingConfiguration : IEntityTypeConfiguration<VideoProcessing>
{
    public void Configure(EntityTypeBuilder<VideoProcessing> builder)
    {
        builder.ToTable("video_processing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.VideoAssetId).HasColumnName("video_asset_id");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.ProgressPercentage).HasColumnName("overall_progress_percentage");
        builder.Property(x => x.IsCriticalError).HasColumnName("is_critical_error");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.MaxRetries).HasColumnName("max_retries");
        builder.Property(x => x.NextRetryAt).HasColumnName("next_retry_at");

        builder.OwnsMany(vp => vp.Steps, sb =>
        {
            sb.ToTable("processing_steps");
            sb.HasKey(s => s.Id);

            sb.WithOwner().HasForeignKey("VideoProcessingId");
            sb.Property<Guid>("VideoProcessingId")
                .HasColumnName("video_processing_id");

            sb.Property(x => x.Order)
                .HasColumnName("order");

            sb.Property(x => x.Weight)
                .HasColumnName("weight");

            sb.Property(x => x.Type)
                .HasConversion<string>()
                .HasColumnName("type");

            sb.Property(x => x.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            sb.Property(x => x.ResultData)
                .HasColumnName("result_data")
                .HasColumnType("jsonb");

            sb.Property(x => x.ErrorMessage)
                .HasColumnName("error_message");

            sb.Property(x => x.StartedAt)
                .HasColumnName("started_at");

            sb.Property(x => x.CompletedAt)
                .HasColumnName("completed_at");

            sb.HasIndex(s => new { s.Type }).HasDatabaseName("ix_processing_steps_step_type");
            sb.HasIndex(s => new { s.Status }).HasDatabaseName("ix_processing_steps_status");
        });
        
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_video_processing_status");
        builder.HasIndex(x => x.VideoAssetId)
            .IsUnique()
            .HasDatabaseName("ux_video_processing_video_asset_id");
        builder.HasIndex(x => new { x.Status, x.StartedAt }).HasDatabaseName("ix_video_processing_status_started_at");
    }
}
