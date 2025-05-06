using FrameHub.Model.Dto.Registration;

namespace FrameHub.Service.Interfaces;

public interface IRegistrationStrategy
{
    Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
}