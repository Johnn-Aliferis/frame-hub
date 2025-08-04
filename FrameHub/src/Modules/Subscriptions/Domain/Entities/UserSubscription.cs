using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Shared.Domain.Entity;

namespace FrameHub.Modules.Subscriptions.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public required string UserId { get; set; }
    public long SubscriptionPlanId { get; set; }
    public string? CustomerId { get; set; }
    public string? SubscriptionId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public virtual ApplicationUser? User { get; set; }
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
}