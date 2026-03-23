using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.ValueObjects;

public record FileName
{
    public string Value { get; }

    public string Extension { get; }

    private FileName() { }
    private FileName(string name,  string extension)
    {
        Value = name;
        Extension = extension;
    }
    
    static Result<FileName, Error> Create(string name, string extension)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsInvalid(nameof(name));

        var dot = name.LastIndexOf('.');
        if (dot == -1 || dot == name.Length - 1)
            return GeneralErrors.ValueIsInvalid("File must have extension");
        
        var extensionLower = name[(dot + 1)..].ToLowerInvariant();
        return new FileName(name, extensionLower);
    }
}

