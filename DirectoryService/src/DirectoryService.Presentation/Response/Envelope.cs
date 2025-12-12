using DirectoryService.Domain.Shared;

namespace DirectoryService.Presentation.Response;

internal record Envelope
{
    public object? Result { get; }

    public Errors? ErrorsList { get; }

    public bool IsError => ErrorsList != null || (ErrorsList != null && ErrorsList.Any());

    public DateTime TimeGenerated { get; }

    public Envelope(object? result, Errors? errorsList)
    {
        Result = result;
        ErrorsList = errorsList;
        TimeGenerated = DateTime.Now;
    }

    public static Envelope Ok(object? result = null) => new(result, null);

    public static Envelope Error(Errors errorsList) => new(null, errorsList);
}

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? ErrorsList { get; }

    public bool IsError => ErrorsList != null || (ErrorsList != null && ErrorsList.Any());

    public DateTime TimeGenerated { get; }

    public Envelope(T? result, Errors? errorsList)
    {
        Result = result;
        ErrorsList = errorsList;
        TimeGenerated = DateTime.Now;
    }

    public static Envelope<T> Ok(T? result = default) => new(result, null);

    public static Envelope<T> Error(Errors errorsList) => new(default, errorsList);
}