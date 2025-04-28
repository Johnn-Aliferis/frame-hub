using FrameHub.Model.Dto;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class DefaultLoginStrategy : ILoginStrategy
{
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        // Default Login to be implemented
        // todo : add log here to see if correctly designed and works.
        throw new NotImplementedException();
    }
}