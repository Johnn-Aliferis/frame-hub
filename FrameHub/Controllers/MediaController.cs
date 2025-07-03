using FrameHub.ActionFilters;
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
    /// <summary>
    /// Generate a presigned Url that front end shall use to upload directly to media provider. Keeps system scalable.
    /// </summary>
    [HttpPost]
    [Route("presigned-url")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<ActionResult<string>> GeneratePresignedUrl([FromBody] PresignedUrlRequestDto presignedUrlRequestDto)
    {
        var userId = HttpContext.Items["UserId"] as string;
        var generatedUrl = await mediaService.GeneratePresignedUrl(userId!, presignedUrlRequestDto);
        return Created(string.Empty, generatedUrl);
    }
    
    /// <summary>
    /// Receives confirmation from front end once the upload is complete and persists photo entity to DB.
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<ActionResult<PhotoResponseDto>> ConfirmUpload([FromBody] PhotoRequestDto photoRequestDto)
    {
        var userId = HttpContext.Items["UserId"] as string;
        var createdMedia = await mediaService.ConfirmMediaUploadAsync(userId!, photoRequestDto);
        return Created(string.Empty, createdMedia);
    }
    
    /// <summary>
    /// Receives a delete request for media and removes photos from selected provider.
    /// </summary>
    [HttpDelete("{photoId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<IActionResult> DeleteMedia(long photoId)
    {
        var userId = HttpContext.Items["UserId"] as string;
        await mediaService.DeleteImage(userId!, photoId);
        return NoContent();
    }
}