using System.Text;
using FrameHub.Modules.Subscriptions.Application.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Stripe;

namespace FrameHub.Modules.Subscriptions.Infrastructure.Messaging.RabbitMQ;

public class StripeWebhookConsumer(
    IRabbitMqConnectionProvider provider,
    IServiceScopeFactory scopeFactory,
    ILogger<StripeWebhookConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await provider.GetChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);

        // Message handling
        consumer.ReceivedAsync += async (model, ea) =>
        {
            using var scope = scopeFactory.CreateScope();
            var consumerService = scope.ServiceProvider.GetRequiredService<IStripeConsumerService>();
            
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var stripeEvent = EventUtility.ParseEvent(message);
                
                await consumerService.HandleMessage(stripeEvent!);
            }
            catch (Exception ex)
            {
                logger.LogError("Something went wrong during message consuming with error : {},", ex.Message);
            }
            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
        };

        // Start the consumer
        await channel.BasicConsumeAsync(
            queue: "stripe_webhook_queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }
}