using FileService.Domain.Assets;
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

            mb.Property(md => md.Status).HasConversion<string>();


        });

    }
}