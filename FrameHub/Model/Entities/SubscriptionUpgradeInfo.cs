namespace FrameHub.Model.Entities;

public class SubscriptionUpgradeInfo : BaseEntity
{
    public required string UserId { get; set; }
    public required string SubscriptionId { get; set; }
    public required string PreviousPriceId { get; set; }
    public required string PreviousPeriodEnd { get; set; }
    public required string NewPriceId { get; set; }
    public required string UpgradeStatus { get; set; }
    
    public virtual required ApplicationUser User { get; set; }
}