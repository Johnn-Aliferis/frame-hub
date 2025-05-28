using RabbitMQ.Client;

namespace FrameHub.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task InitAsync()
    {
        var user = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        var port = Environment.GetEnvironmentVariable("RABBITMQ_PORT");

        var factory = new ConnectionFactory
        {
            Uri = new Uri($"amqp://{user}:{pass}@{host}:{port}/")
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync("stripe_events", ExchangeType.Direct, durable: true, autoDelete: false,
            arguments: null);
        await  _channel.QueueDeclareAsync("stripe_webhook_queue", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        await _channel.QueueBindAsync("stripe_webhook_queue", "stripe_events", "stripe.subscription", arguments: null);
        
        await _channel.CloseAsync();
    }
    
    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection == null)
        {
            await InitAsync();
        }
        return _connection!;
    }
    
    public async Task<IChannel> GetChannelAsync()
    {
        if (_connection == null)
        {
            await InitAsync();
        }
        return await _connection!.CreateChannelAsync(); // for thread safety , safe parallel usage. 
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
        }
        
    }
}