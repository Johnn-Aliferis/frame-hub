using System.Text;
using FrameHub.Infrastructure.Messaging.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Stripe;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController(
    IRabbitMqConnectionProvider brokerProvider,
    ILogger<StripeWebhookController> logger) : ControllerBase
{
    private readonly string _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")!;

    /// <summary>
    /// Controller method to handle webhook events sent by Stripe. 
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];
        var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);

        var channel = await brokerProvider.GetChannelAsync();

        try
        {
            var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(stripeEvent));

            var properties = new BasicProperties { Persistent = true };

            logger.LogInformation("Stripe message with type : {} received , forwarding to message broker...",
                stripeEvent.Type);

            await channel.BasicPublishAsync(
                exchange: "stripe_events",
                routingKey: "stripe.subscription",
                mandatory: false,
                basicProperties: properties,
                body: new ReadOnlyMemory<byte>(message),
                cancellationToken: CancellationToken.None
            );
        }
        finally
        {
            await channel.CloseAsync();
        }

        return Ok(new { status = "received" });
    }
}