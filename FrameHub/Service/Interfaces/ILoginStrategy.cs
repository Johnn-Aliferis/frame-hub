using FrameHub.Model.Dto;

namespace FrameHub.Service.Interfaces;

public interface ILoginStrategy
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
}