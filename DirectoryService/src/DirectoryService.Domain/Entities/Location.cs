using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.Domain.Entities;

public class Location
{
    public Location() { }
    
    private List<DepartmentLocation> _departmentLocations = [];
    private Location(Guid id,Name name, Address address, Timezone timezone, 
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        
    }
    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public Address Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Location> Create(Name name, Address address, Timezone timezone)
    {
        
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        return new Location
        (
            id, 
            name,
            address, 
            timezone, 
            true, 
            createdAt, 
            null);
    }
}

