using System.Text.Json;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Contracts.HttpCommunication;

internal static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<Result<T, Error>> HandleResponseAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        FileServiceEnvelope<T>? envelope;

        try
        {
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            envelope = await JsonSerializer.DeserializeAsync<FileServiceEnvelope<T>>(
                contentStream,
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException)
        {
            return Error.Failure(
                "file-service.invalid-response",
                "File Service returned an invalid response.");
        }

        if (envelope is null)
        {
            return Error.Failure(
                "file-service.empty-response",
                "File Service returned an empty response.");
        }

        var error = envelope.ErrorsList?.FirstOrDefault();
        if (error is not null)
            return error;

        if (!response.IsSuccessStatusCode)
        {
            return Error.Failure(
                "file-service.request-failed",
                $"File Service request failed with status code {(int)response.StatusCode}.");
        }

        if (envelope.Result is null)
        {
            return Error.Failure(
                "file-service.missing-result",
                "File Service returned a successful response without a result.");
        }

        return envelope.Result;
    }

    private sealed record FileServiceEnvelope<T>(
        T? Result,
        List<Error>? ErrorsList);
}
