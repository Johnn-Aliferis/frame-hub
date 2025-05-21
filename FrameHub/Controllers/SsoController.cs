using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/sso/{provider}")]
public class SsoController(ISsoService ssoService) : ControllerBase
{
    [HttpGet("start")]
    [AllowAnonymous]
    public IActionResult StartSso(string provider)
    {
        var ssoChallengeHandler = ssoService.HandleSsoStart(provider, Url);
        return Challenge(ssoChallengeHandler.Properties, ssoChallengeHandler.Provider);
    }

    [HttpGet("callback", Name = "SsoCallback")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromRoute] string provider)
    {
        // todo : use info and pass it down to get the values needed. Middleware already handles the code returned from google , no need to do it myself.
        var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        var result = await ssoService.HandleCallbackAsync(provider.ToLowerInvariant(), "code");
        return Ok(result);
    }
}