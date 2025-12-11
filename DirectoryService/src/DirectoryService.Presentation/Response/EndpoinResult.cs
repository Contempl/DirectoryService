using System.Reflection;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Microsoft.AspNetCore.Http.Metadata;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Presentation.Response;

public sealed class EndpointResult<TValue> : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndpointResult(Result<TValue, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorResult(result.Error);
    }

    public EndpointResult(Result<TValue, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) =>
        _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);

    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);

    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope<TValue>), ["application/json"]));

        builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(401, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(403, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope<TValue>), ["application/json"]));
    }
}