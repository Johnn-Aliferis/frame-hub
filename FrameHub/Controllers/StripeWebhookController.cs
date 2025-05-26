using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    
    // Todo : This is placeholder and default code , to be changes accordingly later. 
    
    private readonly string _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")!; 
    
    
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        var signatureHeader = Request.Headers["Stripe-Signature"];
        var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
        // add try catch to the above for security in headers.   

        Console.WriteLine(stripeEvent);
        // check accordingly which event types we have gotten and perform actions accordingly.
        return Ok();
    }
}