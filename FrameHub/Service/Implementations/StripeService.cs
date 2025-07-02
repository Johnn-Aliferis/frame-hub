using FrameHub.Service.Interfaces;
using Stripe;

namespace FrameHub.Service.Implementations;

public class StripeService(
    CustomerService customerService,
    PaymentMethodService paymentMethodService,
    SubscriptionService subscriptionService,
    InvoiceService invoiceService) : IStripeService
{
    public async Task<string> CreateCustomerAsync(string userId, string email)
    {
        // Create customer in Stripe
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = email,
            Metadata = new Dictionary<string, string> { { "userId", userId } },
            TestClock = "clock_1RgM5bCQhowdgEANycfepNSk" // testing only
        });

        return customer.Id;
    }

    public async Task AttachPaymentMethodAsync(string paymentMethodId, string customerId)
    {
        var paymentMethod = await paymentMethodService.GetAsync(paymentMethodId);

        if (paymentMethod.CustomerId == customerId)
        {
            return;
        }

        await paymentMethodService.AttachAsync(paymentMethodId,
            new PaymentMethodAttachOptions
            {
                Customer = customerId
            });
    }

    public async Task DeleteUserSubscriptionAtEndOfBillingPeriod(string subscriptionId)
    {
        await subscriptionService.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
    }

    public async Task ScheduleNewSubscriptionAtEndOfBillingPeriod(string subscriptionId, string newPlanPriceId)
    {
        var subscription = await subscriptionService.GetAsync(subscriptionId);

        // Update subscription to switch to new plan at period end
        var updateOptions = new SubscriptionUpdateOptions
        {
            Items =
            [
                new SubscriptionItemOptions
                {
                    Id = subscription.Items.Data[0].Id,
                    Price = newPlanPriceId,
                    Quantity = 1
                }
            ],
            ProrationBehavior = "none",
            CancelAtPeriodEnd = false
        };

        updateOptions.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Unchanged;
        await subscriptionService.UpdateAsync(subscriptionId, updateOptions);
    }

    public async Task UpgradeUserSubscriptionAsync(string subscriptionId, string newPlanPriceId)
    {
        var currentSubscription = await subscriptionService.GetAsync(subscriptionId);
        var existingItemId = currentSubscription.Items.Data.First().Id;

        var updateOptions = new SubscriptionUpdateOptions
        {
            Items =
            [
                new SubscriptionItemOptions
                {
                    Id = existingItemId,
                    Price = newPlanPriceId
                }
            ],
            ProrationBehavior = "none",
            CollectionMethod = "charge_automatically",
            BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now
        };
        var updatedSubscription = await subscriptionService.UpdateAsync(subscriptionId, updateOptions);

        // Create separate invoice for the new charge
        await invoiceService.CreateAsync(new InvoiceCreateOptions
        {
            Customer = updatedSubscription.CustomerId,
            Subscription = updatedSubscription.Id,
            AutoAdvance = true
        });
    }

    public async Task RevertUserSubscriptionAsync(string subscriptionId, string previousPriceId,
        DateTime? originalEndPeriod)
    {
        var currentSubscription = await subscriptionService.GetAsync(subscriptionId);
        var existingItemId = currentSubscription.Items.Data.First().Id;

        var revertOptions = new SubscriptionUpdateOptions
        {
            Items =
            [
                new SubscriptionItemOptions
                {
                    Id = existingItemId,
                    Price = previousPriceId
                }
            ],
            ProrationBehavior = "none",
            PaymentBehavior = "allow_incomplete",
            TrialEnd = originalEndPeriod
        };
        await subscriptionService.UpdateAsync(subscriptionId, revertOptions);
    }

    public async Task SetDefaultPaymentMethodAsync(string paymentMethodId, string customerId)
    {
        await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        });
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, string userId, string planName)
    {
        var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = [new SubscriptionItemOptions { Price = priceId }],
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "plan", planName }
            }
        });

        return subscription.Id;
    }

    public async Task<string> CreateTestCardPaymentMethodAsync(string token)
    {
        var paymentMethod = await paymentMethodService.CreateAsync(new PaymentMethodCreateOptions
        {
            Type = "card",
            Card = new PaymentMethodCardOptions
            {
                Token = token
            }
        });
        
        return paymentMethod.Id;
    }
}