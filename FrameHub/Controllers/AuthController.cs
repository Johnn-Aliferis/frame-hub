using System.Security.Claims;
using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILoginService loginService, IRegistrationService registrationService) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var response = await loginService.LoginDefaultAsync(loginRequestDto);
        return Ok(response);
    }

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] DefaultRegistrationRequestDto defaultRegistrationRequestDto)
    {
        var response = await registrationService.RegisterDefaultAsync(defaultRegistrationRequestDto);
        return Ok(response);
    }

    [HttpGet("test", Name = "Test")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult Test()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (userId is null || email is null)
            return Unauthorized("Invalid token");

        return Ok("test");
    }
}