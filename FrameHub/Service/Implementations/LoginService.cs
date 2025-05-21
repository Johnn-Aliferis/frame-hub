using System.Net;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Sso;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Service.Implementations;

public class LoginService(SignInManager<ApplicationUser> signInManager, IUserRepository userRepository) : ILoginService
{
    public async Task<LoginResponseDto> LoginDefaultAsync(LoginRequestDto loginRequestDto)
    {
        var result = await 
            signInManager.PasswordSignInAsync(loginRequestDto.Email, loginRequestDto.Password, false, false);

        if (!result.Succeeded)
        {
            throw new LoginException("Invalid email/password. Please try again", HttpStatusCode.Unauthorized);
        }
        if (result.IsLockedOut)
        {
            throw new LoginException("Account is locked.", HttpStatusCode.Forbidden);
        }
        if (result.IsNotAllowed)
        {
            throw new LoginException("Login not allowed. Email might not be confirmed.", HttpStatusCode.Forbidden);
        }
        if (result.RequiresTwoFactor)
        {
            throw new LoginException("Two-factor authentication required.", HttpStatusCode.Forbidden);
        }
        
        var user = await userRepository.FindUserByEmailAsync(loginRequestDto.Email);
        
        // Checking also status of user
        if (user == null)
        {
            throw new LoginException("User not found", HttpStatusCode.Unauthorized);
        }
        
        // Step 3: Generate JWT 
        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            AccessToken = token
        };
    }

    public LoginResponseDto SsoLogin(ApplicationUser applicationUser)
    {
        var token = GenerateJwtToken(applicationUser);

        return new LoginResponseDto
        {
            AccessToken = token
        };
    }


    private static string GenerateJwtToken(ApplicationUser applicationUser)
    {
        // todo: implement correctly.
        return "AccessToken";
    }
}