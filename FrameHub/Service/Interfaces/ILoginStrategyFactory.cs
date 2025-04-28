namespace FrameHub.Service.Interfaces;

public interface ILoginStrategyFactory
{
    ILoginStrategy DetermineLoginStrategy(string loginMethod);
}