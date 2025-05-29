namespace FrameHub.Model.Entities;

public class UserTransactionHistory : BaseEntity
{
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public string? InvoiceId { get; set; }
    public required string Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public required string UserId { get; set; }
    
    public virtual ApplicationUser? User { get; set; }
}