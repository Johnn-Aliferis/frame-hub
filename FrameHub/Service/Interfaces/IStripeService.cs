using Stripe;

namespace FrameHub.Service.Interfaces;

public interface IStripeService
{
    Task<string> CreateCustomerAsync(string userId, string email);
    Task AttachPaymentMethodAsync(string paymentMethodId, string customerId);
    Task SetDefaultPaymentMethodAsync(string paymentMethodId, string customerId);
    Task<string?> FindCustomerActiveSubscriptionIdAsync(string customerId);
    Task DeleteUserSubscriptionAtEndOfBillingPeriod(string subscriptionId);
    Task DowngradeUserSubscriptionAtEndOfBillingPeriod(string subscriptionId, string currentPlanPriceId, string newPlanPriceId);
    Task UpgradeUserSubscriptionAsync(string subscriptionId, string newPlanPriceId);
    Task HandleFailedSubscriptionUpgrade(string invoiceId, string subscriptionId, string previousPlanPriceId);
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, string userId, string planName);
    Task<string> CreateTestCardPaymentMethodAsync(string token);
}