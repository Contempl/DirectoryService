using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public class ApplicationDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DepartmentConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PositionConfiguration).Assembly);
    }


    public DbSet<Department> Departments  { get; set; }
    
    public DbSet<DepartmentLocation> DepartmentLocations  { get; set; }
    
    public DbSet<DepartmentPosition> DepartmentPositions  { get; set; }
    
    public DbSet<Location> Locations  { get; set; }

    public DbSet<Position> Positions { get; set; }
    
}