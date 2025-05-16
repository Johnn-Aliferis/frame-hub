using System.Net;
using FrameHub.Exceptions;
using FrameHub.Service.Interfaces;
using FrameHub.Service.Strategies;

namespace FrameHub.Service.Factories;

public class SsoProviderStrategyFactory(IServiceProvider serviceProvider) : ISsoProviderStrategyFactory
{
    public ISsoProviderStrategy GetStrategy(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "google" => serviceProvider.GetRequiredService<GoogleSsoProviderStrategy>(),
            // future reference: "microsoft" => ...
            _ => throw new SsoException($"Unsupported SSO provider: {provider}", HttpStatusCode.BadRequest)
        };
    }
}