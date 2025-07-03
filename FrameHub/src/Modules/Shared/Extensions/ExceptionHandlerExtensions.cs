using FrameHub.Modules.Auth.API.ExceptionHandler;
using FrameHub.Modules.Media.API.ExceptionHandler;
using FrameHub.Modules.Shared.API.ExceptionHandlers;

namespace FrameHub.Modules.Shared.Extensions;

public static class ExceptionHandlerExtensions
{
    public static void AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionHandler, GeneralExceptionHandler>();
        services.AddSingleton<IExceptionHandler, RegistrationExceptionHandler>();
        services.AddSingleton<IExceptionHandler, LoginExceptionHandler>();
        services.AddSingleton<IExceptionHandler, MediaExceptionHandler>();
        services.AddSingleton<IExceptionHandler, ProviderExceptionHandler>();
        
        services.AddSingleton<ExceptionHandlingStrategy>();
    }
}