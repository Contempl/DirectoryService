using CSharpFunctionalExtensions;
using FileService.Contracts;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features;

public sealed class StartMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart-upload", async Task<EndpointResult<Guid>> (
            [FromBody] StartMultipartUploadRequest request,
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class StartMultipartUploadHandler
{
    private readonly ILogger<StartMultipartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;

    public StartMultipartUploadHandler(
        ILogger<StartMultipartUploadHandler> logger,
        IS3Provider s3Provider)
    {
        _logger = logger;
        _s3Provider = s3Provider;
    }

    public async Task<Result<Guid, Error>> Handle(StartMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        // выполнить валидацию

        // посчитать количество чанков для загрузки файла и их размер

        // создать доменную сущность MediaAsset, вызвав метод CreateForUpload

        // начать multipart загрузку

        // сгенирировать коллекцию uploadurl для чанков

        // если успешно, то сохранить в базу данных MediaAsset со статусов UPLOADING

        // вернуть данные mediaasset (id), uploadId, коллекцию ссылок для загрузки чанков, размер чанка

        //var startUploadResult = await _s3Provider.StartMultipartUploadAsync(request, cancellationToken);

        return Guid.NewGuid();
    }
}
