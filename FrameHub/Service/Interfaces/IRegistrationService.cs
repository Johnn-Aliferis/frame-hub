using FrameHub.Model.Dto.Registration;

namespace FrameHub.Service.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
}