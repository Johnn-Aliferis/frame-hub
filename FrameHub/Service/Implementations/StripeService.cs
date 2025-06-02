using FrameHub.Service.Interfaces;
using Stripe;

namespace FrameHub.Service.Implementations;

public class StripeService : IStripeService
{
    public async Task<string> CreateCustomerAsync(string userId, string email)
    {
        // Create customer in Stripe
        var customerService = new CustomerService();

        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = email,
            Metadata = new Dictionary<string, string> { { "userId", userId } }
        });

        return customer.Id;
    }

    public async Task AttachPaymentMethodAsync(string paymentMethodId, string customerId)
    {
        var paymentMethodService = new PaymentMethodService();
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

    public async Task<string?> FindCustomerActiveSubscriptionIdAsync(string customerId)
    {
        var subscriptionService = new SubscriptionService();
        var subscriptions = await subscriptionService.ListAsync(new SubscriptionListOptions
        {
            Customer = customerId,
            Status = "active",
            Limit = 1
        });

        var subscription = subscriptions.Data.FirstOrDefault();

        return subscription?.Id;
    }

    public async Task DeleteUserSubscriptionAtEndOfBillingPeriod(string subscriptionId)
    {
        var subscriptionService = new SubscriptionService();
        await subscriptionService.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
    }

    public async Task DowngradeUserSubscriptionAtEndOfBillingPeriod(string subscriptionId, string currentPlanPriceId, string newPlanPriceId)
    {
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(subscriptionId);
        var currentPeriodEnd = subscription.Items.Data.First().CurrentPeriodEnd;

        var scheduleOptions = new SubscriptionScheduleCreateOptions
        {
            FromSubscription = subscriptionId,
            Phases =
            [
                // Phase 1: Keep current plan
                new SubscriptionSchedulePhaseOptions
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = currentPeriodEnd,
                    Items =
                    [
                        new SubscriptionSchedulePhaseItemOptions
                        {
                            Price = currentPlanPriceId
                        }
                    ],
                    ProrationBehavior = "none"
                },

                // Phase 2: Downgrade to another plan
                new SubscriptionSchedulePhaseOptions
                {
                    Items =
                    [
                        new SubscriptionSchedulePhaseItemOptions
                        {
                            Price = newPlanPriceId
                        }
                    ],
                    ProrationBehavior = "none"
                }
            ]
        };
        var scheduleService = new SubscriptionScheduleService();
        await scheduleService.CreateAsync(scheduleOptions);
    }

    public async Task UpgradeUserSubscriptionAsync(string subscriptionId, string newPlanPriceId)
    {
        var subscriptionService = new SubscriptionService();
        var updateOptions = new SubscriptionUpdateOptions
        {
            Items = [
                new SubscriptionItemOptions
                {
                    Price = newPlanPriceId
                }
            ],
            ProrationBehavior = "create_prorations", // Charge immediately
        };
        await subscriptionService.UpdateAsync(subscriptionId, updateOptions);
    }

    public async Task HandleFailedSubscriptionUpgrade(string invoiceId, string subscriptionId, string previousUserPlanPriceId)
    {
        // Do not charge user again after failed payment on upgrade
        var invoiceService = new InvoiceService();
        var subscriptionService = new SubscriptionService();
        await invoiceService.MarkUncollectibleAsync(invoiceId);
        
        // Update stripe plan back to previous and avoid invoicing right away.
        await subscriptionService.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
        {
            Items = [
                new SubscriptionItemOptions
                {
                    Price = previousUserPlanPriceId
                }
            ],
            ProrationBehavior = "none"
        });
    }

    public async Task SetDefaultPaymentMethodAsync(string paymentMethodId, string customerId)
    {
        var customerService = new CustomerService();

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
        var subscriptionService = new SubscriptionService();
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
        var paymentMethodService = new PaymentMethodService();

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