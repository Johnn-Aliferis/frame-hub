using System.Text.Json;
using FrameHub.Modules.Auth.Application.Exception;
using FrameHub.Modules.Shared.API.ExceptionHandlers;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Auth.API.ExceptionHandler;

public class RegistrationExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(RegistrationException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "Registration Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}