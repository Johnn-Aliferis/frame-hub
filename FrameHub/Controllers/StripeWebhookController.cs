using System.Text;
using FrameHub.Infrastructure.Messaging.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Stripe;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController(IRabbitMqConnectionProvider brokerProvider) : ControllerBase
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
            // var invoice = stripeEvent.Data.Object as Invoice;
            // var periodEnd = invoice?.Lines?.Data?.FirstOrDefault()?.Period?.End;
            // billing_reason --> to distinguish event and proceed with our logic as intended.
            var channel = await brokerProvider.GetChannelAsync();
            try
            {
                var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(stripeEvent));

                await channel.BasicPublishAsync(
                    exchange: "stripe_events",
                    routingKey: "stripe.subscription",
                    mandatory: false,
                    basicProperties: new BasicProperties(),
                    body: new ReadOnlyMemory<byte>(message),
                    cancellationToken: CancellationToken.None
                );
            }
            finally
            {
                await channel.CloseAsync();
            }
        }
        return Ok();
    }
}