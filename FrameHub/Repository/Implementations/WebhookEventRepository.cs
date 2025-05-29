using FrameHub.ContextConfiguration;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

public class WebhookEventRepository(AppDbContext context) : IWebhookEventRepository
{
    private readonly DbSet<WebhookEvent> _events = context.Set<WebhookEvent>();

    public async Task PersistWebhookDataAsync(WebhookEvent webhookEvent)
    {
        await _events.AddAsync(webhookEvent);
        await context.SaveChangesAsync();
    }
}