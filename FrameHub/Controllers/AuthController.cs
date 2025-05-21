using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILoginService loginService, IRegistrationService registrationService) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var response = await loginService.LoginDefaultAsync(loginRequestDto);
        return Ok(response);
    }

    [HttpPost]
    [Route("register")]
    public async Task<ActionResult> Register([FromBody] DefaultRegistrationRequestDto defaultRegistrationRequestDto)
    {
        var response = await registrationService.RegisterDefaultAsync(defaultRegistrationRequestDto);
        return Ok(response);
    }
}