using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public IQueryable<MediaAsset> MediaAssetsQuery => MediaAssets.AsQueryable().AsNoTracking();
}