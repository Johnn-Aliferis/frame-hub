using FrameHub.Model.Dto.Subscription;

namespace FrameHub.Service.Interfaces;

public interface IPaymentSubscriptionService
{
    Task<UserSubscriptionDto> CreateSubscriptionAsync(string userId, string email, SubscriptionRequestDto subscriptionRequest);
}