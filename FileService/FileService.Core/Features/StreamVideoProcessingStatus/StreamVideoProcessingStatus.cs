using System.Text.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core.Features.GetVideoProcessingStatus;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Kernel;

namespace FileService.Core.Features.StreamVideoProcessingStatus;

public sealed class StreamVideoProcessingStatusEndpoint : IEndpoint
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/files/{videoAssetId:guid}/processing-status/stream",
            async Task(
                [FromRoute] Guid videoAssetId,
                HttpContext httpContext,
                [FromServices] IServiceScopeFactory scopeFactory,
                [FromServices] IOptions<VideoProcessingStatusStreamOptions> streamOptions,
                CancellationToken cancellationToken) =>
            {
                // FS-13: Интервалы приходят из конфигурации и могут отличаться между окружениями.
                var pollingInterval = TimeSpan.FromSeconds(
                    streamOptions.Value.PollingIntervalSeconds);
                var heartbeatInterval = TimeSpan.FromSeconds(
                    streamOptions.Value.HeartbeatIntervalSeconds);

                // FS-13: Первое чтение выполняем до открытия stream, чтобы неизвестный id вернул обычный 404.
                var initialResult = await ReadStatusAsync(
                    scopeFactory,
                    videoAssetId,
                    cancellationToken);
                if (initialResult.IsFailure)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                httpContext.Response.ContentType = "text/event-stream";
                httpContext.Response.Headers.CacheControl = "no-cache";
                httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

                var lastSentStatus = initialResult.Value;
                await WriteEventAsync(
                    httpContext.Response,
                    "initial",
                    lastSentStatus,
                    cancellationToken);
                var lastWriteAt = DateTime.UtcNow;

                if (lastSentStatus.IsTerminal)
                    return;

                try
                {
                    // FS-13: Соединение остаётся открытым, пока обработка не завершится или клиент не отключится.
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(pollingInterval, cancellationToken);

                        var currentResult = await ReadStatusAsync(
                            scopeFactory,
                            videoAssetId,
                            cancellationToken);
                        if (currentResult.IsFailure)
                            continue;

                        var currentStatus = currentResult.Value;

                        // FS-13: Одинаковые снимки не отправляем, чтобы не создавать клиенту лишние события.
                        if (currentStatus == lastSentStatus)
                        {
                            // FS-13: Комментарий поддерживает тихое соединение и не создаёт progress event.
                            if (DateTime.UtcNow - lastWriteAt >= heartbeatInterval)
                            {
                                await WriteHeartbeatAsync(httpContext.Response, cancellationToken);
                                lastWriteAt = DateTime.UtcNow;
                            }

                            continue;
                        }

                        var eventName = currentStatus.IsTerminal ? "final" : "progress";
                        await WriteEventAsync(
                            httpContext.Response,
                            eventName,
                            currentStatus,
                            cancellationToken);

                        lastSentStatus = currentStatus;
                        lastWriteAt = DateTime.UtcNow;

                        if (currentStatus.IsTerminal)
                            return;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // FS-13: Закрытие страницы завершает только SSE-запрос и не отменяет Quartz processing job.
                }
            });
    }

    private static async Task<Result<VideoProcessingStatusResponse, Error>> ReadStatusAsync(
        IServiceScopeFactory scopeFactory,
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        // FS-13: Новый scope даёт свежий DbContext и позволяет увидеть изменения, записанные Quartz job.
        await using var scope = scopeFactory.CreateAsyncScope();
        var statusReader = scope.ServiceProvider.GetRequiredService<VideoProcessingStatusReader>();

        return await statusReader.ReadAsync(videoAssetId, cancellationToken);
    }

    private static async Task WriteEventAsync<T>(
        HttpResponse response,
        string eventName,
        T data,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);

        await response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteHeartbeatAsync(
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        // Строка, начинающаяся с ':', является комментарием по формату SSE.
        await response.WriteAsync(": heartbeat\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
