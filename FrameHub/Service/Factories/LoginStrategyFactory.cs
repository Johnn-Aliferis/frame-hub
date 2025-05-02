using FrameHub.Service.Interfaces;
using FrameHub.Service.Strategies;

namespace FrameHub.Service.Factories;

public class LoginStrategyFactory(IServiceProvider serviceProvider) : ILoginStrategyFactory
{
    public ILoginStrategy DetermineLoginStrategy(string loginMethod)
    {
        // Since we are dealing with Factory Design Pattern , we let DI container take care of the dependencies in
        // our strategies so that we don't have to manually create and pass them down -- Tight coupling --
        // i.e New DefaultLoginStrategy(userRepository); etc
        
        return loginMethod.ToLower() switch
        {
            "default" => serviceProvider.GetRequiredService<DefaultLoginStrategy>(),
            "google" => serviceProvider.GetRequiredService<GoogleLoginStrategy>(),
            _ => throw new ArgumentException($"Unsupported login method: {loginMethod}")
        };
        // Todo : Add above a custom exception via our middleware for better error readability.
    }
}