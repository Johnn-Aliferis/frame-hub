using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class SsoController(ISsoService ssoService) : ControllerBase
{
    [HttpGet("sso/start")]
    [AllowAnonymous]
    public IActionResult StartSso([FromQuery] string provider)
    {
        var ssoChallengeHandler = ssoService.HandleSsoStart(provider, Url);
        return Challenge(ssoChallengeHandler.Properties, ssoChallengeHandler.Provider);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("google-sso-callback", Name = "google")]
    public async Task<ActionResult> Register([FromQuery] string code, [FromQuery] string provider)
    {
        var result = await ssoService.HandleCallbackAsync(provider.ToLowerInvariant(), code);
        return Ok(result);
    }
}