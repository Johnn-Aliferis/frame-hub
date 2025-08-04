using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Domain.Entities;

namespace FrameHub.Modules.Auth.Application.Services;

public interface IRegistrationService
{
    Task<RegistrationResponseDto> RegisterDefaultAsync(DefaultRegistrationRequestDto defaultRegistrationRequestDto);
    Task<ApplicationUser> RegisterSsoAsync(SsoRegistrationRequestDto ssoRegistrationRequestDto);
}