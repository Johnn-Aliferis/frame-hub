using System.Net;
using FrameHub.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    [HttpGet]
    [Route("/test")]
    public ActionResult<string> Get()
    {
        throw new GeneralException("Test exception", HttpStatusCode.InternalServerError);
        return "Hello from /test!";
    }
}