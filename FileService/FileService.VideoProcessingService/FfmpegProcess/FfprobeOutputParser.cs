using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.Kernel;

namespace FileService.VideoProcessing.FfmpegProcess;

public static class FfprobeOutputParser
{
    public static Result<VideoMetadata, Error> Parse(string jsonOutput)
    {
        if (string.IsNullOrWhiteSpace(jsonOutput))
            return FileErrors.InvalidFfprobeOutput("Empty output");

        FfprobeResponse? response;

        try
        {
            response = JsonSerializer.Deserialize<FfprobeResponse>(jsonOutput);
        }
        catch (JsonException exception)
        {
            return FileErrors.InvalidFfprobeOutput($"JSON parse error: {exception.Message}");
        }

        if (response is null)
            return FileErrors.InvalidFfprobeOutput("Null response");

        var stream = response.Streams?.FirstOrDefault();
        if (stream is null)
            return FileErrors.InvalidFfprobeOutput("No video stream found");

        if (stream.Width is null || stream.Height is null)
            return FileErrors.InvalidFfprobeOutput("Missing resolution");

        var durationSeconds = response.Format?.Duration;
        if (durationSeconds is null || durationSeconds <= 0)
            return FileErrors.InvalidFfprobeOutput("Missing or invalid duration");

        var duration = TimeSpan.FromSeconds(durationSeconds.Value);

        return VideoMetadata.Create(duration, stream.Width.Value, stream.Height.Value);
    }

    private sealed class FfprobeResponse
    {
        [JsonPropertyName("streams")]
        public List<StreamInfo>? Streams { get; set; }

        [JsonPropertyName("format")]
        public FormatInfo? Format { get; set; }
    }

    private sealed class StreamInfo
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    private sealed class FormatInfo
    {
        [JsonPropertyName("duration")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Duration { get; set; }
    }
}
