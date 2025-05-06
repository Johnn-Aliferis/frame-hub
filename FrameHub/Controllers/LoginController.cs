using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class LoginController(ILoginService loginService) : ControllerBase
{
    [HttpPost]
    [Route("/login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var response =  await loginService.LoginAsync(loginRequestDto);
        return Ok(response);
    }
    
    // [HttpPost]
    // [Route("/register")]
    // public async Task<ActionResult> Login([FromBody] RegistrationRequestDto registrationRequestDto)
    // {
    //     var response =  await loginService.LoginAsync(loginRequestDto);
    //     return Ok(response);
    // }
}