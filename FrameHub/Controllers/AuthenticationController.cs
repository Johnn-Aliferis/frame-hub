using System.Net;
using FrameHub.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class AuthenticationController(ILogger<AuthenticationController> logger) : ControllerBase
{
    [HttpPost]
    [Route("/login")]
    public ActionResult<string> Get()
    {
        throw new GeneralException("Test exception", HttpStatusCode.InternalServerError);
        return "Hello from /test!";
    }
}