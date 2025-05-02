using FrameHub.Model.Dto.Login;

namespace FrameHub.Service.Interfaces;

public interface ILoginService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
}