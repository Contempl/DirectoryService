using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IReadDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DepartmentConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PositionConfiguration).Assembly);
    }


    public DbSet<Department> Departments  { get; set; }
    
    public DbSet<DepartmentLocation> DepartmentLocations  { get; set; }
    
    public DbSet<DepartmentPosition> DepartmentPositions  { get; set; }
    
    public DbSet<Location> Locations  { get; set; }

    public DbSet<Position> Positions { get; set; }

    public IQueryable<Department> DepartmentsRead  => Set<Department>().AsNoTracking();
    public IQueryable<DepartmentLocation> DepartmentLocationsRead => Set<DepartmentLocation>().AsNoTracking();
    public IQueryable<DepartmentPosition> DepartmentPositionsRead => Set<DepartmentPosition>().AsNoTracking();
    public IQueryable<Location> LocationsRead => Set<Location>().AsNoTracking();
    public IQueryable<Position> PositionsRead => Set<Position>().AsNoTracking();
}

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DirectoryService.Presentation"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}