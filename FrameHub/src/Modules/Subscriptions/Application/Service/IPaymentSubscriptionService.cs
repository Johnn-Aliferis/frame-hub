using FrameHub.Modules.Subscriptions.API.DTO;

namespace FrameHub.Modules.Subscriptions.Application.Service;

public interface IPaymentSubscriptionService
{
    Task<UserSubscriptionDto> CreateSubscriptionAsync(string userId, string email, SubscriptionRequestDto subscriptionRequest);
    Task UpdateSubscriptionAsync(long userSubscriptionId ,string userId, string email, SubscriptionRequestDto subscriptionRequest);
    Task DeleteSubscriptionAsync(long userSubscriptionId, string userId);
}