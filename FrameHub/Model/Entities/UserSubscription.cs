namespace FrameHub.Model.Entities;

public class UserSubscription : BaseEntity
{
    public int UserId { get; set; }
    public required string SubscriptionPlan { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public required User User { get; set; }
}