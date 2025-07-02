using System.Security.Claims;
using FrameHub.Model.Dto.Media;
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
    [Route("presigned")]
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
        return Created(string.Empty, generatedUrl);
    }
    
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ConfirmUpload([FromBody] PhotoRequestDto photoRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        var createdMedia = await mediaService.ConfirmMediaUploadAsync(userId, photoRequestDto);
        return Created(string.Empty, createdMedia);
    }
    
    [HttpDelete("{photoId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteMedia(long photoId)
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