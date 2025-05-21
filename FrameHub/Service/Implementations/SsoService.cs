using System.Net;
using FrameHub.Enum;
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

        if (!System.Enum.GetNames(typeof(SsoProvider))
                .Any(name => name.Equals(provider, StringComparison.OrdinalIgnoreCase)))

        {
            throw new SsoException($"Unsupported provider", HttpStatusCode.BadRequest);
        }

        var redirectUrl = url.Action(
            action: "Register",
            controller: "Sso",
            values: new { provider = provider },
            protocol: "https"
        );

        if (string.IsNullOrEmpty(redirectUrl))
        {
            throw new SsoException("Invalid or unsupported provider", HttpStatusCode.BadRequest);
        }

        Console.WriteLine("Redirect URI: " + redirectUrl);

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
        };


        return new SsoChallengeResultDto { Properties = props, Provider = provider };
    }


    public async Task<LoginResponseDto> HandleCallbackAsync(string provider, string code)
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
            throw new SsoException("This email is already registered with a different provider.",
                HttpStatusCode.Conflict);
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

    private async Task<LoginResponseDto> HandleSsoLogin(UserInfoSsoResponseDto userInfo)
    {
        var user = await userManager.FindByLoginAsync(
            userInfo.ExternalLoginInfo.LoginProvider,
            userInfo.ExternalLoginInfo.ProviderKey);

        if (user == null)
        {
            throw new SsoException("User not found for external login", HttpStatusCode.Unauthorized);
        }

        return loginService.SsoLogin(user);


        // for now to simply test login works , next up set up JWT etc.
    }
}