using FrameHub.ExceptionHandlers;

namespace FrameHub.Extensions;

public static class ExceptionHandlerExtensions
{
    public static void AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionHandler, GeneralExceptionHandler>();
        services.AddSingleton<IExceptionHandler, RegistrationExceptionHandler>();
        
        services.AddSingleton<ExceptionHandlingStrategy>();
    }
}