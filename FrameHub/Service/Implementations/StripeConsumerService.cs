using System.Net;
using AutoMapper;
using FrameHub.Exceptions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;
using Stripe;

namespace FrameHub.Service.Implementations;

public class StripeConsumerService(
    IWebhookEventRepository webhookEventRepository,
    IMapper mapper,
    IUserRepository userRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IUnitOfWork unitOfWork,
    ILogger<StripeConsumerService> logger)
    : IStripeConsumerService
{
    public async Task HandleMessage(Event? stripeEvent)
    {
        if (stripeEvent is null)
        {
            throw new StripeConsumerException("Message received was null", HttpStatusCode.BadRequest);
        }

        // Todo : Check if event already exists in our db. If it does acknowledge the message as duplicate and move on.

        // Persist audit log in our db.
        await unitOfWork.BeginTransactionAsync();
        try
        {
            await PersistWebhookData(stripeEvent);
            // Handle different stripe event types later 
            if (stripeEvent.Type == "invoice.payment_succeeded")
            {
                await HandleSubscriptionCreation(stripeEvent);
            }
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "An error occurred during creation of subscription.");
            throw new StripeConsumerException(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    private async Task PersistWebhookData(Event stripeEvent)
    {
        var webhookEvent = mapper.Map<WebhookEvent>(stripeEvent);
        await webhookEventRepository.PersistWebhookDataAsync(webhookEvent);
    }

    private async Task HandleSubscriptionCreation(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;

        ValidateInvoiceData(invoice);

        // subscription_create only for subscription create flow for billing reason.
        var email = invoice!.CustomerEmail;
        var userSubscription = await userRepository.FindUserSubscriptionByUserEmailAsync(email);
        if (userSubscription is null)
        {
            throw new StripeConsumerException("Cannot find user subscription associated with given email",
                HttpStatusCode.BadRequest);
        }

        var requestedPlan = invoice.Lines?.Data?.FirstOrDefault()?.Pricing?.PriceDetails?.Price;

        var existingPlan = await subscriptionPlanRepository.FindSubscriptionPlanByPriceIdAsync(requestedPlan!);
        if (existingPlan is null)
        {
            throw new StripeConsumerException("Cannot find requested plan", HttpStatusCode.BadRequest);
        }

        await PersistUserSubscription(userSubscription, existingPlan, invoice);
        await PersistUserTransactionHistory(invoice, userSubscription.UserId);
        
        await unitOfWork.CommitAsync();
        
        // Optional future enhancment --> Send email.
    }

private async Task PersistUserSubscription(UserSubscription userSubscription, SubscriptionPlan existingPlan,
    Invoice invoice)
{
    userSubscription.SubscriptionPlanId = existingPlan.Id;
    userSubscription.AssignedAt = invoice.Lines?.Data?.FirstOrDefault()?.Period.Start;
    userSubscription.ExpiresAt = invoice.Lines?.Data?.FirstOrDefault()?.Period.End;
    userSubscription.SubscriptionId =
        invoice.Lines?.Data?.FirstOrDefault()?.Parent?.SubscriptionItemDetails?.Subscription;
    await userRepository.SaveUserSubscriptionAsync(userSubscription);
}

private async Task PersistUserTransactionHistory(Invoice invoice, string userId)
{
    var userTransactionHistory = new UserTransactionHistory
    {
        Amount = invoice.AmountPaid,
        Currency = invoice.Currency,
        InvoiceId = invoice.Id,
        Description = "PaymentSuccess",
        ReceiptUrl = invoice.HostedInvoiceUrl,
        UserId = userId
    };
    await userRepository.SaveUserTransactionHistoryAsync(userTransactionHistory);
}


private static void ValidateInvoiceData(Invoice? invoice)
{
    if (invoice is null)
    {
        throw new StripeConsumerException("Invoice received is null", HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(invoice.CustomerEmail))
    {
        throw new StripeConsumerException("Message does not contain user information", HttpStatusCode.BadRequest);
    }
}

}