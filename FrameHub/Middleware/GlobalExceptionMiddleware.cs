using FrameHub.ExceptionHandlers;

namespace FrameHub.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    ExceptionHandlingStrategy exceptionHandlingStrategy)

{
    public async Task InvokeAsync(HttpContext httpContext) 
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await exceptionHandlingStrategy.HandleResponseAsync(httpContext, ex);
        }
    }
}