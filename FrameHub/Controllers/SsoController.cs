using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/sso/{provider}")]
public class SsoController(ISsoService ssoService) : ControllerBase
{
    /// <summary>
    /// Method for starting sso flow.
    /// </summary>
    [HttpGet("start")]
    [AllowAnonymous]
    public IActionResult StartSso(string provider)
    {
        var ssoChallengeHandler = ssoService.HandleSsoStart(provider, Url);
        return Challenge(ssoChallengeHandler.Properties, ssoChallengeHandler.Provider);
    }

    /// <summary>
    /// Callback method that sso provider calls back.
    /// </summary>
    [HttpGet("callback", Name = "SsoCallback")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromRoute] string provider)
    {
        var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        var result = await ssoService.HandleCallbackAsync(info);
        return Ok(result);
    }
}