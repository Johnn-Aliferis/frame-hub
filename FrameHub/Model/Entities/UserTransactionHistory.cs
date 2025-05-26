namespace FrameHub.Model.Entities;

public class UserTransactionHistory : BaseEntity
{
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string StripePaymentIntentId { get; set; }
    public required string StripeInvoiceId { get; set; }
    public required string Description { get; set; }
    public required string ReceiptUrl { get; set; }
    public required string MetadataJson { get; set; }
    public required string UserId { get; set; }
    
    public virtual required ApplicationUser User { get; set; }
}