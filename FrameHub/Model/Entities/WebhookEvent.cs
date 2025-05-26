namespace FrameHub.Model.Entities;

public class WebhookEvent
{
    public long Id { get; set; }
    public required string EventId { get; set; }
    public required string EventType { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public required string RawPayload { get; set; }
    public bool Processed { get; set; } = true;
}