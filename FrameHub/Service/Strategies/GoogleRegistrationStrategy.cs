using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class GoogleRegistrationStrategy : IRegistrationStrategy
{
    public async Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        // Google SSO Registration to be implemented.
        throw new NotImplementedException();
    }
}