namespace FrameHub.Service.Interfaces;

public interface IRegistrationStrategyFactory
{
    IRegistrationStrategy DetermineRegistrationStrategy(string registrationMethod);
}