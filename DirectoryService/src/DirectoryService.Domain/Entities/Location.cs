using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.VO;
using Shared.Kernel;

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

    public LocationPhoto? Photo { get; private set; }

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
            createdAt);
    }
    
    public UnitResult<Error> Update(Name name, Address address, Timezone timezone)
    {
        Name = name;

        Address = address;

        Timezone = timezone;
        
        UpdatedAt =  DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SoftDelete()
    {
        IsActive = false;
        
        UpdatedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Restore()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AttachPhoto(LocationPhoto photo)
    {
        if (Photo is not null)
            return LocationPhotoErrors.AlreadyAttached();

        Photo = photo;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ReplacePhoto(LocationPhoto photo)
    {
        if (Photo is null)
            return LocationPhotoErrors.NotAttached();

        Photo = photo;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemovePhoto()
    {
        if (Photo is null)
            return LocationPhotoErrors.NotAttached();

        Photo = null;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
    
}

