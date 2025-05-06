using FrameHub.Service.Interfaces;
using FrameHub.Service.Strategies;

namespace FrameHub.Service.Factories;

public class RegistrationStrategyFactory(IServiceProvider serviceProvider) : IRegistrationStrategyFactory
{
    public IRegistrationStrategy DetermineRegistrationStrategy(string registrationMethod)
    {
        return registrationMethod.ToLower() switch
        {
            "default" => serviceProvider.GetRequiredService<DefaultRegistrationStrategy>(),
            "google" => serviceProvider.GetRequiredService<GoogleRegistrationStrategy>(),
            _ => throw new ArgumentException($"Unsupported registration method: {registrationMethod}")
        };
        // Todo : Add above a custom exception via our middleware for better error readability.
    }
}