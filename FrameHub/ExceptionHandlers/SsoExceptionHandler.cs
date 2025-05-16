using System.Text.Json;
using FrameHub.Exceptions;

namespace FrameHub.ExceptionHandlers;

public class SsoExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(RegistrationException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "Single Sign on Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}