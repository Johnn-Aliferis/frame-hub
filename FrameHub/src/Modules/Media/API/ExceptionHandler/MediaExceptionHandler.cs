using System.Text.Json;
using FrameHub.Modules.Media.Application.Exception;
using FrameHub.Modules.Shared.API.ExceptionHandlers;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Media.API.ExceptionHandler;

public class MediaExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(MediaException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "Media Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}