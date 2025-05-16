using System.Net;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Dto.Sso;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class SsoService(
    ISsoProviderStrategyFactory ssoProviderStrategyFactory,
    IUserRepository userRepository,
    IRegistrationService registrationService,
    ILoginService loginService) : ISsoService
{
    public async Task<AuthResponseDto> HandleCallbackAsync(string provider, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(provider))
        {
            throw new SsoException("Missing code or provider", HttpStatusCode.BadRequest);
        }

        // Get dynamically the strategy based on provider
        var strategy = ssoProviderStrategyFactory.GetStrategy(provider);
        var userInfo = await strategy.GetUserInfoAsync();

        var user = await userRepository.FindUserByEmailAsync(userInfo.Email);
        var isNewUser = user == null;

        if (isNewUser)
        {
           return await HandleSsoRegistration(userInfo);
        }
        else
        {
            return  await HandleSsoLogin(userInfo);
        }
    }

    private async Task<AuthResponseDto> HandleSsoRegistration(UserInfoSsoResponseDto userInfo)
    {
        var password = Guid.NewGuid().ToString("N")[..10]; // Random SSO password - not for actual use
        var registrationRequestDto = new RegistrationRequestDto
        {
            RegistrationMethod = userInfo.Provider,
            Email = userInfo.Email,
            Password = password,
            DisplayName = $"{userInfo.FirstName ?? ""} {userInfo.LastName ?? ""}".Trim(),
        };
            
        var registeredUser =  await registrationService.RegisterAsync(registrationRequestDto);

        return new AuthResponseDto
        {
            UserId   = registeredUser.UserId,
            Email = registeredUser.Email,
            UserName = registeredUser.UserName,
        };
    }

    private async Task<AuthResponseDto> HandleSsoLogin(UserInfoSsoResponseDto userInfo)
    {
        var logInRequest = new LoginRequestDto
        {
            LoginMethod = userInfo.Provider,
            Email = userInfo.Email,
        };
        var loggedUser =  await loginService.LoginAsync(logInRequest);
        return new AuthResponseDto
        {
            AccessToken   = loggedUser.AccessToken,
            RefreshToken = loggedUser.RefreshToken,
        };
    }
}