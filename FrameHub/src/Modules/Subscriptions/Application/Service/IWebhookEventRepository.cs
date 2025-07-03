using FrameHub.Modules.Subscriptions.Domain.Entities;

namespace FrameHub.Modules.Subscriptions.Application.Service;

public interface IWebhookEventRepository
{
    Task PersistWebhookDataAsync(WebhookEvent webhookEvent);
    Task<WebhookEvent?> FindWebhookEventByEventIdAsync(string id);
}