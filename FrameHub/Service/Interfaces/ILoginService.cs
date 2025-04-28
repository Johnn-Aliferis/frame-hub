using FrameHub.Model.Dto;

namespace FrameHub.Service.Interfaces;

public interface ILoginService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
}