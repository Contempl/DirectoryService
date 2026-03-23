using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.ValueObjects;

public record StorageKey
{
    public string Key { get; }

    public string Prefix { get; }

    public string Location { get; }

    public string Value { get; }

    public string FullPath { get; }

    private StorageKey() { }
    
    private StorageKey(string location, string key, string prefix)
    {
        Location =  location;
        Key = key;
        Prefix = prefix;
        Value = string.IsNullOrEmpty(prefix) ? Key : $"{Prefix}/{Key}";
        FullPath =  $"{Location}/{Value}";
    }
    
    public static Result<StorageKey, Error> Create(string location, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(location))
            return GeneralErrors.ValueIsInvalid("location");

        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;

        Result<string, Error> normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;

        return new StorageKey(location.Trim(), normalizedPrefixResult.Value, normalizedKeyResult.Value);
    }

    public Result<StorageKey, Error> AppendSegment(string segment)
    {
        var normalizedKeyResult = NormalizeSegment(segment);
        if (normalizedKeyResult.IsFailure)
        {
            return normalizedKeyResult.Error;
        }

        return new StorageKey(Location, normalizedKeyResult.Value, Value);
    }

    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        string[] parts = prefix.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            Result<string, Error> normalizedPart = NormalizeSegment(part);
            if (normalizedPart.IsFailure)
                return normalizedPart;

            if (!string.IsNullOrEmpty(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }

        return string.Join('/', normalizedParts);
    }

    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("key");

        string trimmed = value.Trim();

        if (trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid("key");

        return trimmed;
    }
}