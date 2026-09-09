using FileService.Core.Database;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext, IReadDbContext
{
    public const string WOLVERINE_SCHEMA = "wolverine";
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) 
        : base(options) { }
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<VideoProcessing> VideoProcesses => Set<VideoProcessing>();

    public IQueryable<MediaAsset> MediaAssetsQuery => MediaAssets.AsQueryable().AsNoTracking();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<StorageKey>(); // без этого миграция не накатывается из-за того что мы ручками настроили сериализацию в конфигурации
        
        modelBuilder.ApplyConfiguration(new MediaAssetConfiguration());
        modelBuilder.ApplyConfiguration(new VideoAssetConfiguration());
        modelBuilder.ApplyConfiguration(new VideoProcessingConfiguration());

        modelBuilder.MapWolverineEnvelopeStorage(WOLVERINE_SCHEMA);
    }
}
