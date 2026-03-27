using System.Net;
using Microsoft.AspNetCore.Http;

namespace Framework.Response;

public class SuccessResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Ok();

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}

public class SuccessResult<TValue> : IResult
{
    private readonly TValue _value;

    public SuccessResult(TValue value)
    {
        _value = value;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var envelope = Envelope.Ok(_value);

        context.Response.StatusCode = (int)HttpStatusCode.OK;

        return context.Response.WriteAsJsonAsync(envelope);
    }
}