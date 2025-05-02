using FrameHub.Model.Dto.Login;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class DefaultLoginStrategy : ILoginStrategy
{
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        // Default Login to be implemented
        throw new NotImplementedException();
    }
}