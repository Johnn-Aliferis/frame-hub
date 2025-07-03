using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Subscriptions.API.DTO;

public class SubscriptionRequestDto
{
    [Required]
    public required string PaymentMethodId { get; set; }
    
    [Required] 
    public required string PriceId { get; set; }
    
    [Required] 
    public required string PlanName { get; set; }
}