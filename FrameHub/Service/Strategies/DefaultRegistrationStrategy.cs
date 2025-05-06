using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class DefaultRegistrationStrategy : IRegistrationStrategy
{
    public Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        // Default Registration to be implemented
        throw new NotImplementedException();
    }
}