using RabbitMQ.Client;

namespace FrameHub.Modules.Subscriptions.Infrastructure.Messaging.RabbitMQ;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
    Task<IChannel> GetChannelAsync();
}