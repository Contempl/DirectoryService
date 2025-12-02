using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities;

public class Location
{
    private Location(Guid id,Name name, string address, Timezone timezone, 
        bool isActive, IEnumerable<Department> departments, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        Departments = departments.ToList();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        
    }
    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public List<Department> Departments { get; set; }

    public string Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Location> Create(Name name, string address, Timezone timezone, 
        bool isActive, IEnumerable<Department> departments)
    {
        if (!departments.Any())
            return Result.Failure<Location>("Department list is empty");
        
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        return new Location
        (
            id, 
            name,
            address, 
            timezone, 
            true, 
            departments, 
            createdAt, 
            null);
    }
}

public record Timezone
{
    private Timezone(string timezone)
    {
        Value = timezone;
    }
    
    public string Value { get; }

    public static Result<Timezone> Create(string windowsId, string? region)
    {
        var convertionResult = TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, region, out var ianaId);
        if (!convertionResult is false || ianaId is null)
            return Result.Failure<Timezone>("Can't convert windows id to IanaId. Please try again.");

        return new Timezone(ianaId);
    }
}