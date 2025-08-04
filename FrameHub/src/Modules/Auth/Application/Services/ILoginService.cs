using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Domain.Entities;

namespace FrameHub.Modules.Auth.Application.Services;

public interface ILoginService
{
    Task<LoginResponseDto> LoginDefaultAsync(LoginRequestDto loginRequestDto);
    LoginResponseDto SsoLogin(ApplicationUser application);
}