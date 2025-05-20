using System.Net;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Dto.Sso;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Implementations;

public class SsoService(
    ISsoProviderStrategyFactory ssoProviderStrategyFactory,
    IUserRepository userRepository,
    IRegistrationService registrationService,
    UserManager<ApplicationUser> userManager,
    ILoginService loginService) : ISsoService
{
    public SsoChallengeResultDto HandleSsoStart(string provider, IUrlHelper url)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new SsoException("Provider is required", HttpStatusCode.BadRequest);
        }

        var providerRoute = provider.ToLowerInvariant();
        var redirectUrl = url.RouteUrl(providerRoute, new { provider });

        if (string.IsNullOrEmpty(redirectUrl))
        {
            throw new SsoException("Invalid or unsupported provider", HttpStatusCode.BadRequest);
        }

        var props = new AuthenticationProperties { RedirectUri = redirectUrl };

        return new SsoChallengeResultDto { Properties = props, Provider = providerRoute };
    }


    public async Task<AuthResponseDto> HandleCallbackAsync(string provider, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(provider))
        {
            throw new SsoException("Missing code or provider", HttpStatusCode.BadRequest);
        }

        // Get dynamically the strategy based on provider(Google , Microsoft) -- may have different claim set up
        var strategy = ssoProviderStrategyFactory.GetStrategy(provider);
        var userInfo = await strategy.GetUserInfoAsync();

        
        var userByLogin = await userManager.FindByLoginAsync(
            userInfo.ExternalLoginInfo.LoginProvider,
            userInfo.ExternalLoginInfo.ProviderKey
        );
        
        if (userByLogin != null)
        {
            return await HandleSsoLogin(userInfo);
        }
        
        var userByEmail = await userRepository.FindUserByEmailAsync(userInfo.Email);
        if (userByEmail != null)
        {
            // Same email but from different provider detected.
            throw new SsoException("This email is already registered with a different provider.", HttpStatusCode.Conflict);
        }
        
        await HandleSsoRegistration(userInfo);

        return await HandleSsoLogin(userInfo);
    }

    private async Task HandleSsoRegistration(UserInfoSsoResponseDto userInfo)
    {
        var ssoRegistrationRequest = new SsoRegistrationRequestDto
        {
            Email = userInfo.Email,
            DisplayName = userInfo.Email
        };
        
        await registrationService.RegisterSsoAsync(ssoRegistrationRequest);
    }

    private async Task<AuthResponseDto> HandleSsoLogin(UserInfoSsoResponseDto userInfo)
    {
        Console.WriteLine("Logged in!");
        
        var logInRequest = new LoginRequestDto
        {
            LoginMethod = userInfo.Provider,
            Email = userInfo.Email,
        };
        var loggedUser = await loginService.LoginAsync(logInRequest);
        return new AuthResponseDto
        {
            AccessToken = loggedUser.AccessToken,
            RefreshToken = loggedUser.RefreshToken,
        };
        
    }
}