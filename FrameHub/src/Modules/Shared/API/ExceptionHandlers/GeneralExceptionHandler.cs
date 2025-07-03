using System.Text.Json;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Shared.API.ExceptionHandlers;

public class GeneralExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(GeneralException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "General Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}