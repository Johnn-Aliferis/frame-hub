using System.Security.Claims;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    [HttpPost]
    [Route("upload-url")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GeneratePresignedUrl()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        var generatedUrl = await mediaService.GeneratePresignedUrl(userId, email);
        return Ok(generatedUrl);
    }
    
    [HttpPost]
    [Route("confirm")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ConfirmUpload()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        // todo : Add class implementation here 
        return Ok("Db altered, photo with id : temp, is now successfully uploaded");
    }
    
    [HttpDelete("{photoId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ConfirmUpload(long photoId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        // todo : Add class implementation here 
        return Ok("Image successfully deleted");
    }
}