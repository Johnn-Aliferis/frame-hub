using RabbitMQ.Client;

namespace FrameHub.Infrastructure.Messaging.RabbitMQ;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
    Task<IChannel> GetChannelAsync();
}