using System.Text.Json;
using FileService.Domain.Assets;
using FileService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(ma => ma.Id);

        builder.HasDiscriminator<string>("asset_type")
            .HasValue<VideoAsset>("video")
            .HasValue<PreviewAsset>("preview");

        builder.OwnsOne(ma => ma.MediaData, mb =>
        {
            mb.ToJson("media_data");

            mb.OwnsOne(med => med.ContentType, cb =>
            {
                cb.Property(x => x.MediaType).HasConversion<string>().HasColumnName("media_type");
                cb.Property(x => x.Value).HasColumnName("value");
            });

            mb.OwnsOne(md => md.FileName, fn =>
            {
                fn.Property(x => x.Extension).HasColumnName("extension");
                fn.Property(x => x.Value).HasColumnName("value");
            });

            mb.Property(md => md.Size).HasColumnName("size");
            mb.Property(md => md.ExpectedChunksCount).HasColumnName("expected_chunks_count");
        });
        
        builder.Property(ma => ma.Id).HasColumnName("id");
        
        builder.Property(ma => ma.Status).HasConversion<string>().HasColumnName("status");

        builder.OwnsOne(ma => ma.Owner, ob =>
        {
            ob.Property(o => o.Context).HasColumnName("context");
            ob.Property(o => o.EntityId).HasColumnName("context_id");
        });
        
        builder.Property(ma => ma.AssetType).HasConversion<string>().HasColumnName("asset_type");
        
        builder.Property(ma => ma.CreatedAt).HasColumnName("created_at");
        builder.Property(ma => ma.UpdatedAt).HasColumnName("updated_at");
        
        builder.Property(ma => ma.RawKey)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null)!)
            .HasColumnName("raw_key")
            .HasColumnType("jsonb");
        
        builder.Property(ma => ma.FinalKey)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null)!)
            .HasColumnName("final_key")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.HasIndex(ma => new
        {
            ma.Status, ma.CreatedAt
        });
    }
}