using FrameHub.Middleware;

namespace FrameHub.Extensions;

public static class MiddlewareExtensions
{
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        // Exception Handling Middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}