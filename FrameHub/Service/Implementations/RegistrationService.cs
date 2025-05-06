using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class RegistrationService(IRegistrationStrategyFactory registrationStrategyFactory) : IRegistrationService
{
    public async Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        var registrationStrategy = registrationStrategyFactory.DetermineRegistrationStrategy(registrationRequestDto.RegistrationMethod);
        var registrationResponse = await registrationStrategy.RegisterAsync(registrationRequestDto);
        
        // todo: handle here the response etc
        throw new NotImplementedException();
    }
}