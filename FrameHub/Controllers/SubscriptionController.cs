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
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequestDto createSubscriptionRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized("User claims missing or invalid.");
        }
        
        var userSubscription =  await paymentSubscriptionService.CreateSubscriptionAsync(userId, email, createSubscriptionRequestDto);
        
        return Ok(userSubscription);
    }
}