using FrameHub.Model.Dto.Login;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class AuthenticationController(ILoginService loginService) : ControllerBase
{
    [HttpPost]
    [Route("/login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var response =  await loginService.LoginAsync(loginRequestDto);
        return Ok(response);
    }
}