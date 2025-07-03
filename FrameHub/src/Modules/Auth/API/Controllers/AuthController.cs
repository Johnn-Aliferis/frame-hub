using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Modules.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILoginService loginService, IRegistrationService registrationService) : ControllerBase
{
    /// <summary>
    /// Logs in the user and returns a JWT token.
    /// </summary>
    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var response = await loginService.LoginDefaultAsync(loginRequestDto);
        return Ok(response);
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] DefaultRegistrationRequestDto defaultRegistrationRequestDto)
    {
        var response = await registrationService.RegisterDefaultAsync(defaultRegistrationRequestDto);
        return Created(string.Empty,response);
    }
}