namespace FrameHub.Model.Entities;

public class SubscriptionPlans : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int MaxUploads { get; set; }
    public decimal MonthlyPrice { get; set; }
}