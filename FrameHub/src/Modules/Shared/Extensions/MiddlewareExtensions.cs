using FrameHub.Modules.Shared.Infrastructure.Middleware;

namespace FrameHub.Modules.Shared.Extensions;

public static class MiddlewareExtensions
{
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        // Exception Handling Middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}