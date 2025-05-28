using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FrameHub.Infrastructure.Messaging.RabbitMQ;

public class StripeWebhookConsumer(IRabbitMqConnectionProvider provider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await provider.GetChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);

        // Message handling -- add db logic later.
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine("Received message: {0}", message);
            Console.WriteLine("Model?: {0}", model);

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