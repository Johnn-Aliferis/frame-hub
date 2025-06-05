using System.Security.Claims;
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
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequestDto subscriptionRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        var userSubscription =  await paymentSubscriptionService.CreateSubscriptionAsync(userId, email, subscriptionRequestDto);
        
        return Ok(userSubscription);
    }
    
    [HttpPut("{userSubscriptionId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateSubscription(long userSubscriptionId, [FromBody] SubscriptionRequestDto subscriptionRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        await paymentSubscriptionService.UpdateSubscriptionAsync(userSubscriptionId,userId, email, subscriptionRequestDto);
        
        return Ok("Subscription update request succeeded, you will soon be notified via email");
    }
    
    
    [HttpDelete("{userSubscriptionId:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteSubscription(long userSubscriptionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        await paymentSubscriptionService.DeleteSubscriptionAsync(userSubscriptionId, userId);
        return Ok("Subscription will be cancelled at the end of the current billing cycle. You may continue using your features until date is due");
    }
}