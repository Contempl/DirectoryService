using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Entities.VO;

public record Timezone
{
    private Timezone(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Timezone> Create(string windowsId, string? region)
    {
        var conversionResult = TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, region, out var ianaId);
        if (!conversionResult is false || ianaId is null)
            return Result.Failure<Timezone>("Can't convert windows id to IanaId. Please try again.");

        return new Timezone(ianaId);
    }
}