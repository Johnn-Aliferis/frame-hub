namespace FrameHub.Service.Interfaces;

public interface ISsoProviderStrategyFactory
{
    ISsoProviderStrategy GetStrategy(string provider);
}