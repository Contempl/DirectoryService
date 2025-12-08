using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities.VO;

public record Path
{
    private Path(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Path> Create(Guid id, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            return Result.Failure<Path>("Path cannot be empty and between 150 characters");
        
        return new Path(value);
    }
}