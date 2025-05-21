using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Sso;
using FrameHub.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Service.Interfaces;

public interface ILoginService
{
    Task<LoginResponseDto> LoginDefaultAsync(LoginRequestDto loginRequestDto);
    LoginResponseDto SsoLogin(ApplicationUser application);
}