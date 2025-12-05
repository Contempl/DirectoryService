using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities.VO;

public record Name
{
    private Name(string name)
    {
        Value = name;
    }
    public string Value { get; }

    public static Result<Name> Create(Guid id, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Name>("Name must be not empty and between 150 characters");
        return new Name(value);
    }
}