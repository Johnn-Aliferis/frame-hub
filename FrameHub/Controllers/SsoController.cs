using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
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
        // Front-end provides the sso provider
        var redirectUrl = Url.Action("Callback", "SsoCallback", new { provider });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(props, provider);
    }
    
    [HttpGet]
    [AllowAnonymous]
    [Route("google-sso-callback")]
    public async Task<ActionResult> Register([FromQuery] string code, [FromQuery] string provider)
    {
        var result = await ssoService.HandleCallbackAsync(provider.ToLowerInvariant(), code);
        return Ok(result);
    }
}