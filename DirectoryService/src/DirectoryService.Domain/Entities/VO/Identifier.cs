using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities.VO;

public record Identifier
{
    private Identifier(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Identifier> Create(Guid id, string value)
    {
        foreach (var letter in value.ToCharArray())
        {
            if (!char.IsAsciiLetterOrDigit(letter))
                return Result.Failure<Identifier>("Value must contain only Latin characters and numbers");
        }
        
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Identifier>("Identifier must be not empty and between 150 characters");

        return new Identifier(value);
    }
}