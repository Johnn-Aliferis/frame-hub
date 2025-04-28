namespace FrameHub.Model.Entities;

public class SubscriptionPlan : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int MaxUploads { get; set; }
    public decimal MonthlyPrice { get; set; }
    
    public virtual required UserSubscription UserSubscription { get; set; }
}