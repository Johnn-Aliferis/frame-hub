using System.Net;
using FrameHub.Exceptions;
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
            _ => throw new RegistrationException($"Unsupported registration method: {registrationMethod}", HttpStatusCode.BadRequest)
        };
    }
}