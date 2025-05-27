using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly string _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")!; 
    
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];
        
        var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
        if (stripeEvent.Type == "invoice.payment_succeeded")
        {
            Console.WriteLine(stripeEvent);
            var invoice = stripeEvent.Data.Object as Invoice;
            var periodEnd = invoice?.Lines?.Data?.FirstOrDefault()?.Period?.End;
            Console.WriteLine(JsonConvert.SerializeObject(invoice, Formatting.Indented));
            // check the stripe Event logged .
            // billing_reason --> to distinguish event and proceed with our logic as intended.
        }
        return Ok();
    }
}