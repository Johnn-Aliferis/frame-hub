using System.Text.Json;

namespace FrameHub.Modules.Shared.API.ExceptionHandlers;

public class ExceptionHandlingStrategy
{
    private readonly Dictionary<Type, IExceptionHandler> _handlers;

    public ExceptionHandlingStrategy(IEnumerable<IExceptionHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.ExceptionType, h => h);
    }

    public async Task HandleResponseAsync(HttpContext context, Exception exception)
    {
        var exceptionType = exception.GetType();
        context.Response.ContentType = "application/json";

        if (_handlers.TryGetValue(exceptionType, out var handler))
        {
            await handler.HandleResponseAsync(context, exception);
        }
        else
        {
            context.Response.StatusCode = 500;
            var response = new
                { error = $"An Exception of {exception.GetType()} occurred.", details = exception.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}