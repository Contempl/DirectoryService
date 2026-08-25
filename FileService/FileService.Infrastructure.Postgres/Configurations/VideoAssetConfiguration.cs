using System.Text.Json;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public sealed class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.Property(video => video.Metadata)
            .HasConversion(
                value => value == null
                    ? null
                    : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => value == null
                    ? null
                    : JsonSerializer.Deserialize<VideoMetadata>(value, (JsonSerializerOptions?)null))
            .HasColumnName("video_metadata")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(video => video.HlsRootKey)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<StorageKey>(value, (JsonSerializerOptions?)null)!)
            .HasColumnName("hls_root_key")
            .HasColumnType("jsonb");
    }
}
