using FrameHub.Model.Entities;
using Stripe;

namespace FrameHub.Repository.Interfaces;

public interface IWebhookEventRepository
{
    Task PersistWebhookDataAsync(WebhookEvent webhookEvent);
}