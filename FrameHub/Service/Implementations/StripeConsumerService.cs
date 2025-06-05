using System.Net;
using AutoMapper;
using FrameHub.Enum;
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
    IStripeService stripeService,
    ILogger<StripeConsumerService> logger)
    : IStripeConsumerService
{
    private const string PaymentSuccessEventType = "invoice.payment_succeeded";
    private const string SubscriptionDeleteEventType = "customer.subscription.deleted";
    private const string PaymentFailedEventType = "invoice.payment_failed";

    public async Task HandleMessage(Event? stripeEvent)
    {
        if (stripeEvent is null)
        {
            throw new StripeConsumerException("Message received was null", HttpStatusCode.BadRequest);
        }

        // Handle duplicated Event
        var webhookEvent = await webhookEventRepository.FindWebhookEventByEventIdAsync(stripeEvent.Id);
        if (webhookEvent is not null)
        {
            return;
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await PersistWebhookData(stripeEvent);
            switch (stripeEvent.Type)
            {
                case PaymentSuccessEventType:
                    await HandleInvoicePaymentSuccess(stripeEvent);
                    break;

                case SubscriptionDeleteEventType:
                    await HandleCustomerSubscriptionDelete(stripeEvent);
                    break;

                case PaymentFailedEventType:
                    await HandleFailedSubscriptionUpdate(stripeEvent);
                    break;
            }

            await unitOfWork.CommitAsync();
            // Optional future enhancment --> Send email.
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "An error occurred handling of payment provider response.");
            throw new StripeConsumerException(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    private async Task HandleInvoicePaymentSuccess(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;

        ValidateInvoiceData(invoice);

        if (invoice!.BillingReason.Equals("subscription_create")
            || invoice.BillingReason.Equals("subscription_update")
            || invoice.BillingReason.Equals("subscription_cycle"))
        {
            await HandleSubscriptionCreation(invoice);
        }
    }

    private async Task HandleCustomerSubscriptionDelete(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;

        ValidateInvoiceCancellationData(subscription);

        await HandleSubscriptionCancellation(subscription!);
    }

    private async Task HandleFailedSubscriptionUpdate(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        ValidateInvoiceData(invoice);

        if (invoice!.BillingReason.Equals("subscription_update"))
        {
            var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.Parent?.SubscriptionItemDetails?.Subscription;
            if (subscriptionId is null)
            {
                throw new StripeConsumerException("User does not have an active subscription in Stripe",
                    HttpStatusCode.BadRequest);
            }
            var currentActivePlan = await userRepository.FindUserSubscriptionByUserEmailAsync(invoice.CustomerEmail);
            await stripeService.RevertUserSubscriptionAsync(subscriptionId,currentActivePlan!.SubscriptionPlan!.PriceId,currentActivePlan.ExpiresAt);
        }
        else
        {
            if (!invoice.BillingReason.Equals("subscription_cycle"))
            {
                var subscriptionService = new SubscriptionService();
                var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.Parent?.SubscriptionItemDetails?.Subscription;

                await subscriptionService.CancelAsync(subscriptionId);

                var userSubscription = await userRepository.FindUserSubscriptionByCustomerIdAsync(invoice.CustomerId);
                var basicSubscription = await FindBasicSubscriptionPlan();

                await CancelUserSubscription(userSubscription!, basicSubscription);
            }
        }
    }

    private async Task PersistWebhookData(Event stripeEvent)
    {
        var webhookEvent = mapper.Map<WebhookEvent>(stripeEvent);
        await webhookEventRepository.PersistWebhookDataAsync(webhookEvent);
    }

    private async Task HandleSubscriptionCreation(Invoice invoice)
    {
        var email = invoice.CustomerEmail;
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
            throw new StripeConsumerException("Cannot find requested plan in DB", HttpStatusCode.BadRequest);
        }

        await PersistUserSubscription(userSubscription, existingPlan, invoice);
        await PersistUserTransactionHistory(invoice, userSubscription.UserId, true);
    }

    private async Task HandleSubscriptionCancellation(Subscription subscription)
    {
        var customerId = subscription.CustomerId;
        var userSubscription = await userRepository.FindUserSubscriptionByCustomerIdAsync(customerId);

        if (userSubscription is null)
        {
            throw new StripeConsumerException("Cannot find user subscription associated with given information",
                HttpStatusCode.BadRequest);
        }
        // Downgrade to Basic plan.
        var subscriptionPlan = await FindBasicSubscriptionPlan();

        await CancelUserSubscription(userSubscription, subscriptionPlan);
        await PersistUserTransactionHistory(null, userSubscription.UserId, false);
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

    private async Task CancelUserSubscription(UserSubscription userSubscription, SubscriptionPlan existingPlan)
    {
        userSubscription.SubscriptionPlanId = existingPlan.Id;
        userSubscription.AssignedAt = DateTime.UtcNow;
        userSubscription.ExpiresAt = null;
        userSubscription.SubscriptionId = null;
        await userRepository.SaveUserSubscriptionAsync(userSubscription);
    }

    private async Task PersistUserTransactionHistory(Invoice? invoice, string userId, bool isCreation)
    {
        UserTransactionHistory userTransactionHistory;
        var requestedPlan = invoice?.Lines?.Data?.FirstOrDefault()?.Pricing?.PriceDetails?.Price;

        if (isCreation)
        {
            userTransactionHistory = new UserTransactionHistory
            {
                Amount = (decimal)invoice!.AmountPaid / 100,
                Currency = invoice.Currency,
                InvoiceId = invoice.Id,
                Description = "Subscription Created/Paid ",
                ReceiptUrl = invoice.HostedInvoiceUrl,
                UserId = userId,
                CreatedAt = DateTime.Now,
                PlanPriceId = requestedPlan
            };
        }
        else
        {
            userTransactionHistory = new UserTransactionHistory
            {
                Description = "Subscription Deleted",
                UserId = userId,
                CreatedAt = DateTime.Now
            };
        }

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

        if (string.IsNullOrWhiteSpace(invoice.BillingReason))
        {
            throw new StripeConsumerException("Message does not contain Billing Reason", HttpStatusCode.BadRequest);
        }
    }

    private static void ValidateInvoiceCancellationData(Subscription? subscription)
    {
        if (subscription is null)
        {
            throw new StripeConsumerException("Subscription received is null", HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            throw new StripeConsumerException("Message does not contain user information", HttpStatusCode.BadRequest);
        }
    }

    private async Task<SubscriptionPlan> FindBasicSubscriptionPlan()
    {
        var subscriptionPlan =
            await subscriptionPlanRepository.FindSubscriptionPlanByIdAsync((long)SubscriptionPlanId.Basic);
        if (subscriptionPlan is null)
        {
            throw new StripeConsumerException("Cannot find requested plan in DB", HttpStatusCode.BadRequest);
        }

        return subscriptionPlan;
    }
}