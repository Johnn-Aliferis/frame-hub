using FrameHub.Model.Dto;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class LoginService(ILoginStrategyFactory loginStrategyFactory) : ILoginService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
    {
        // Todo : Possible handling of validations
        var loginStrategy = loginStrategyFactory.DetermineLoginStrategy(loginRequestDto.LoginMethod);
        var loginResponse = await loginStrategy.LoginAsync(loginRequestDto);
        
        // todo: handle here the response etc
        throw new NotImplementedException();
    }
}