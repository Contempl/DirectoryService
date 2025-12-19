using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Entities.VO;

public record Name
{
    private Name(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return GeneralErrors.ValueIsInvalid("Name must be not empty and between 150 characters");
        return new Name(value);
    }
}