using FrameHub.Modules.Shared.Domain.Entity;

namespace FrameHub.Modules.Subscriptions.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string ProductId { get; set; }
    public required string PriceId { get; set; }
    public string? Description { get; set; }
    public int MaxUploads { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int PlanOrder { get; set; }
    
    public virtual required UserSubscription UserSubscription { get; set; }
}