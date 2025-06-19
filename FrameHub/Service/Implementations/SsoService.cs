using System.Net;
using System.Security.Claims;
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

        if (!System.Enum.GetNames(typeof(SsoProviderEnum))
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

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
        };


        return new SsoChallengeResultDto { Properties = props, Provider = provider };
    }


    public async Task<LoginResponseDto> HandleCallbackAsync(AuthenticateResult result)
    {
        var userInfo = ExtractDataFromResult(result);

        var userByLogin = await userManager.FindByLoginAsync(userInfo.LoginProvider, userInfo.ProviderKey);

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
            DisplayName = userInfo.Email,
            LoginProvider = userInfo.LoginProvider,
            ProviderKey = userInfo.ProviderKey,
        };

        await registrationService.RegisterSsoAsync(ssoRegistrationRequest);
    }

    private async Task<LoginResponseDto> HandleSsoLogin(UserInfoSsoResponseDto userInfo)
    {
        var user = await userManager.FindByLoginAsync(userInfo.LoginProvider, userInfo.ProviderKey);

        if (user == null)
        {
            throw new SsoException("User not found for external login", HttpStatusCode.Unauthorized);
        }
        return loginService.SsoLogin(user);
    }


    private static UserInfoSsoResponseDto ExtractDataFromResult(AuthenticateResult result)
    {
        if (result == null)
        {
            throw new SsoException("Failed to retrieve external login info from provider", HttpStatusCode.BadRequest);
        }
        
        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var lastName = result.Principal?.FindFirstValue(ClaimTypes.Surname);
        var firstName = result.Principal?.FindFirstValue(ClaimTypes.GivenName);
        var providerKey = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (result.Properties?.Items.TryGetValue(".AuthScheme", out var loginProvider) != true || loginProvider == null)
        {
            throw new SsoException("Login provider not found in authentication result.", HttpStatusCode.BadRequest);
        }  
        
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new SsoException("Email not found in sso response", HttpStatusCode.BadRequest);
        }
        
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new SsoException("provider key not found sso response", HttpStatusCode.BadRequest);
        }
        
        return new UserInfoSsoResponseDto
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            LoginProvider = loginProvider,
            ProviderKey = providerKey
        };
    }
}