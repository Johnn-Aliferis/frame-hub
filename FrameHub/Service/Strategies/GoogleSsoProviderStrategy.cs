using System.Net;
using System.Security.Claims;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Sso;
using FrameHub.Model.Entities;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Service.Strategies;

public class GoogleSsoProviderStrategy(SignInManager<ApplicationUser> signInManager) : ISsoProviderStrategy
{
    private const string SsoProvider = "sso";
    
    public async Task<UserInfoSsoResponseDto> GetUserInfoAsync()
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            throw new SsoException("Failed to retrieve external login info from Google", HttpStatusCode.BadRequest);
        }
        
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
        
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new SsoException("Email not found in Google response", HttpStatusCode.BadRequest);
        }
        
        return new UserInfoSsoResponseDto
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Provider = SsoProvider,
            ExternalLoginInfo = info
        };
    }
}