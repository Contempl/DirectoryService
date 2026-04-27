using Microsoft.AspNetCore.Builder;

namespace Framework.Middleware;

public static class UseDevAuthentication
{
    public static IApplicationBuilder UseDevAuth(this WebApplication app)
    {
        return app.UseMiddleware<DevAuthMiddleware>();
    }
}