using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Shared.Domain.Entity;

namespace FrameHub.Modules.Subscriptions.Domain.Entities;

public class UserTransactionHistory : BaseEntity
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? InvoiceId { get; set; }
    public required string Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? PlanPriceId { get; set; }
    public required string UserId { get; set; }
    
    public virtual ApplicationUser? User { get; set; }
}