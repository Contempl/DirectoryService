using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Entities.VO;

public record Path
{
    private Path(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return GeneralErrors.ValueIsInvalid("Path cannot be empty and between 150 characters");
        
        return new Path(value);
    }
}