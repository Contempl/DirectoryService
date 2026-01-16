using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Entities.VO;

public record Timezone
{
    private Timezone(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Result<Timezone, Error> Create(string windowsId)
    {
        var conversionResult = TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, null , out var ianaId);
        if (conversionResult is false || ianaId is null)
            return GeneralErrors.ValueIsInvalid("Invalid windows id provided");

        return new Timezone(ianaId);
    }
}