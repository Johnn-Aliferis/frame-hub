using FrameHub.Model.Dto.Registration;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class RegistrationController(IRegistrationService registrationService) : ControllerBase
{
    [HttpPost]
    [Route("/register")]
    public async Task<ActionResult> Login([FromBody] RegistrationRequestDto registrationRequestDto)
    {
        var response =  await registrationService.RegisterAsync(registrationRequestDto);
        return Ok(response);
    }
}