using FrameHub.Model.Dto;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class GoogleLoginStrategy : ILoginStrategy
{
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        // Google SSO Login to be implemented.
        // todo : add log here to see if correctly designed and works.
        throw new NotImplementedException();
    }
}