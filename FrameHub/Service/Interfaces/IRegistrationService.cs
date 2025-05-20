using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Entities;

namespace FrameHub.Service.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResponseDto> RegisterDefaultAsync(DefaultRegistrationRequestDto defaultRegistrationRequestDto);
    Task<ApplicationUser> RegisterSsoAsync(SsoRegistrationRequestDto ssoRegistrationRequestDto);
}