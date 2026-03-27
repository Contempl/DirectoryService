using Microsoft.AspNetCore.Routing;

namespace Framework.Response;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
