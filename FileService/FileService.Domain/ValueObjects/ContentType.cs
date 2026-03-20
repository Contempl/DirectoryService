using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using Shared.Kernel;

namespace FileService.Domain.ValueObjects;

public record ContentType
{
    public string Value { get; }

    public MediaType MediaType { get;  }
    
    private  ContentType(string value, MediaType type)
    {
        Value = value;
        MediaType = type;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        MediaType category = contentType.ToLowerInvariant() switch
        {
            _ when contentType.Contains("video") => MediaType.VIDEO,
            _ when contentType.Contains("image") => MediaType.IMAGE,
            _ when contentType.Contains("audio") => MediaType.AUDIO,
            _ when contentType.Contains("document") => MediaType.DOCUMENT,
            _ => MediaType.UNKNOWN
        };
        
        return new ContentType(contentType, category);
    }
}