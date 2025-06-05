using Stripe;

namespace FrameHub.Service.Interfaces;

public interface IStripeService
{
    Task<string> CreateCustomerAsync(string userId, string email);
    Task AttachPaymentMethodAsync(string paymentMethodId, string customerId);
    Task SetDefaultPaymentMethodAsync(string paymentMethodId, string customerId);
    Task DeleteUserSubscriptionAtEndOfBillingPeriod(string subscriptionId);
    Task ScheduleNewSubscriptionAtEndOfBillingPeriod(string subscriptionId, string newPlanPriceId);
    Task UpgradeUserSubscriptionAsync(string subscriptionId, string newPlanPriceId);
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, string userId, string planName);
    Task<string> CreateTestCardPaymentMethodAsync(string token);
    Task RevertUserSubscriptionAsync(string subscriptionId, string previousPriceId, DateTime? originalEndPeriod);
}