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
        
        Console.WriteLine("Attached customer: " + paymentMethod.CustomerId);
        
        if (paymentMethod.CustomerId == customerId)
        {
            Console.WriteLine($"Payment method {paymentMethodId} already attached to customer {customerId}.");
            return; // No need to re-attach
        }
        
        await paymentMethodService.AttachAsync(paymentMethodId,
            new PaymentMethodAttachOptions
            {
                Customer = customerId
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

    
    
    public async Task<string> CreateTestCardPaymentMethodAsync()
    {
        var paymentMethodService = new PaymentMethodService();
        var paymentMethod = await paymentMethodService.CreateAsync(new PaymentMethodCreateOptions
        {
            Type = "card",
            Card = new PaymentMethodCardOptions
            {
                Number = "4242424242424242",
                ExpMonth = 12,
                ExpYear = 2026,
                Cvc = "123"
            }
        });

        return paymentMethod.Id;
    }
}