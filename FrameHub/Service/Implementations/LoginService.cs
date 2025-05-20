using FrameHub.Model.Dto.Login;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class LoginService() : ILoginService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        throw new NotImplementedException();
    }
}