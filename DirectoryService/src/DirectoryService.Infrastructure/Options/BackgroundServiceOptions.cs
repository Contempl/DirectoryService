namespace DirectoryService.Infrastructure.Options;

public record BackgroundServiceOptions 
{
    public const string SectionName = "BackgroundJobs";

    public int DepartmentCleanupIntervalHours { get; set; }
    public int HardDeleteThresholdMonths { get; set; }
}