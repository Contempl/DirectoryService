using FileService.Core.Database;
using FileService.Domain.Assets;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext, IReadDbContext
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) 
        : base(options) { }
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public IQueryable<MediaAsset> MediaAssetsQuery => MediaAssets.AsQueryable().AsNoTracking();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<StorageKey>(); // без этого миграция не накатывается из-за того что мы ручками настроили сериализацию в конфигурации
        
        modelBuilder.ApplyConfiguration(new MediaAssetConfiguration());
        modelBuilder.ApplyConfiguration(new VideoProcessingConfiguration());
    }
}
