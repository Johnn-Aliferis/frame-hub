using FrameHub.ActionFilters;
using FrameHub.Model.Dto.Subscription;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController(IPaymentSubscriptionService paymentSubscriptionService) : ControllerBase
{
    /// <summary>
    /// Creates a user subscription.Includes creating a user in stripe , attaching payment method and paying.
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<ActionResult<UserSubscriptionDto>> CreateSubscription([FromBody] SubscriptionRequestDto subscriptionRequestDto)
    {
        var userId = HttpContext.Items["UserId"] as string;
        var email = HttpContext.Items["Email"] as string;
        
        var userSubscription =  await paymentSubscriptionService.CreateSubscriptionAsync(userId!, email!, subscriptionRequestDto);
        return Created(string.Empty,userSubscription);
    }
    
    /// <summary>
    /// Updates a user's subscription. Can either be downgraded or upgraded.
    /// </summary>
    [HttpPut("{userSubscriptionId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<IActionResult> UpdateSubscription(long userSubscriptionId, [FromBody] SubscriptionRequestDto subscriptionRequestDto)
    {
        var userId = HttpContext.Items["UserId"] as string;
        var email = HttpContext.Items["Email"] as string;
        
        await paymentSubscriptionService.UpdateSubscriptionAsync(userSubscriptionId,userId!, email!, subscriptionRequestDto);
        
        return Accepted(new { message = "Subscription update is in progress. You’ll be notified when it’s complete." });
    }
    
    /// <summary>
    /// Delete a user's subscription. Treated as downgrade to basic plan.
    /// </summary>
    [HttpDelete("{userSubscriptionId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserClaim]
    public async Task<IActionResult> DeleteSubscription(long userSubscriptionId)
    {
        var userId = HttpContext.Items["UserId"] as string;
        await paymentSubscriptionService.DeleteSubscriptionAsync(userSubscriptionId, userId!);
        return Accepted(new { message = "Subscription cancellation scheduled. Your access remains until the end of the billing cycle." });
    }
}