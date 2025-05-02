using FrameHub.Model.Dto.Login;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class GoogleLoginStrategy : ILoginStrategy
{
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        // Google SSO Login to be implemented.
        throw new NotImplementedException();
    }
}