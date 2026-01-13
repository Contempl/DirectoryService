using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Entities.VO;

public record Address(string City, string Street, string House, string? Apartment)
{
    public static Result<Address, Error> Create(string city, string street, string house, string? apartment)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(house))
            return GeneralErrors.ValueIsInvalid("Invalid address");
        
        return new Address(city, street, house, apartment);
    }
}
