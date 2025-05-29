namespace FrameHub.Model.Dto.Subscription;

public class UserSubscriptionDto
{
    public required string UserId { get; set; } 
    public long SubscriptionPlanId { get; set; }
    
    public string? CustomerId { get; set; }
    public string? SubscriptionId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string? UserEmail { get; set; }
    public string? PlanName { get; set; }
}