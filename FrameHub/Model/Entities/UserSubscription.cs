namespace FrameHub.Model.Entities;

public class UserSubscription : BaseEntity
{
    public long UserId { get; set; }
    public long SubscriptionPlanId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public virtual required User User { get; set; }
    public virtual required SubscriptionPlan SubscriptionPlan { get; set; }
}