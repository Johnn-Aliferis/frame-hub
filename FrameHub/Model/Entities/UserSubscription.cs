using Microsoft.AspNetCore.Identity;

namespace FrameHub.Model.Entities;

public class UserSubscription : BaseEntity
{
    public required string UserId { get; set; }
    public long SubscriptionPlanId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public virtual required ApplicationUser User { get; set; }
    public virtual required SubscriptionPlan SubscriptionPlan { get; set; }
}