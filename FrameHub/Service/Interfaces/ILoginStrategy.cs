using FrameHub.Model.Dto.Login;

namespace FrameHub.Service.Interfaces;

public interface ILoginStrategy
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
}