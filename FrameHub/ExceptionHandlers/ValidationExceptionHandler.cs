using System.Text.Json;
using FrameHub.Exceptions;

namespace FrameHub.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(ValidationException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "Validation Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}