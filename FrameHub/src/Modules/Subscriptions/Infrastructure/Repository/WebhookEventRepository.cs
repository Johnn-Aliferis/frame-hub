using FrameHub.Modules.Shared.Infrastructure.Persistence;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Modules.Subscriptions.Infrastructure.Repository;

public class WebhookEventRepository(AppDbContext context) : IWebhookEventRepository
{
    private readonly DbSet<WebhookEvent> _events = context.Set<WebhookEvent>();

    public async Task PersistWebhookDataAsync(WebhookEvent webhookEvent)
    {
        await _events.AddAsync(webhookEvent);
        await context.SaveChangesAsync();
    }

    public async Task<WebhookEvent?> FindWebhookEventByEventIdAsync(string eventId)
    {
       return await _events.FirstOrDefaultAsync(webhookEvent => webhookEvent.EventId == eventId);
    }
}