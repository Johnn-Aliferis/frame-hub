namespace FrameHub.Modules.Shared.API.ExceptionHandlers;

public interface IExceptionHandler
{
    Type ExceptionType { get; }
    Task HandleResponseAsync(HttpContext context, Exception exception);
}