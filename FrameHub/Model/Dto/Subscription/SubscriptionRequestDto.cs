using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Subscription;

public class SubscriptionRequestDto
{
    [Required]
    public required string PaymentMethodId { get; set; }
    
    [Required] 
    public required string PriceId { get; set; }
    
    [Required] 
    public required string PlanName { get; set; }
}