using System.Text.Json;
using FrameHub.Modules.Shared.API.ExceptionHandlers;
using FrameHub.Modules.Shared.Application.Exception;
using FrameHub.Modules.Subscriptions.Application.Exception;

namespace FrameHub.Modules.Subscriptions.API.ExceptionHandler;

public class StripeConsumerExceptionHandler : IExceptionHandler
{
    public Type ExceptionType => typeof(StripeConsumerException);

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var ex = (GeneralException)exception;
        context.Response.StatusCode = (int)ex.Status;
        
        var response = new { error = "Stripe Consumer Exception occurred.", details = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}