using System.Net;
using AutoMapper;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Subscription;
using FrameHub.Model.Entities;
using FrameHub.Repository.Implementations;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class PaymentSubscriptionService(
    IUserRepository userRepository,
    IMapper mapper,
    IStripeService stripeService,
    ISubscriptionPlanRepository subscriptionPlanRepository)
    : IPaymentSubscriptionService
{
    public async Task<UserSubscriptionDto> CreateSubscriptionAsync(string userId, string email,
        SubscriptionRequestDto subscriptionRequest)
    {
        // todo : REMOVE BELOW LINE FOR PRODUCTION , RIGHT NOW ONLY FOR TEST
        subscriptionRequest.PaymentMethodId = await stripeService.CreateTestCardPaymentMethodAsync("tok_visa");
        var currentSubscription = await ValidateUserSubscriptionAsync(userId);

        var result = await HandleStripeSubscription(currentSubscription!, userId, email, subscriptionRequest);
        return mapper.Map<UserSubscriptionDto>(result);
    }

    public async Task UpdateSubscriptionAsync(string userId, string email,
        SubscriptionRequestDto subscriptionRequest)
    {
        var currentSubscription = await userRepository.FindUserSubscriptionByUserEmailAsync(email);
        var requestedSubscription =
            await subscriptionPlanRepository.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId);

        if (requestedSubscription is null)
        {
            throw new ValidationException("A subscription with the requested priceId does not exist.",
                HttpStatusCode.BadRequest);
        }

        if (currentSubscription!.SubscriptionPlan!.PlanOrder == requestedSubscription.PlanOrder)
        {
            // Edge case - future handling : User tries to re-subscribe to same plan , possibly after requesting downgrade
            throw new ValidationException("Same plan request currently not supported", HttpStatusCode.BadRequest);
        }

        if (currentSubscription.SubscriptionPlan.PlanOrder > requestedSubscription.PlanOrder)
        {
            await DowngradeSubscription(requestedSubscription,currentSubscription, userId);
        }
        else if (currentSubscription.SubscriptionPlan.PlanOrder < requestedSubscription.PlanOrder)
        {
            await UpgradeSubscription(requestedSubscription,currentSubscription, userId);
        }
    }

    private async Task DowngradeSubscription(SubscriptionPlan requestedSubscription,
        UserSubscription currentSubscription, string userId)
    {
        //  Downgrade to basic plan --> Delete subscription
        if (requestedSubscription.Id.Equals((long)SubscriptionPlanId.Basic))
        {
            await stripeService.DeleteUserSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!);
            await AuditTransactionHistory("User Deletion Requested", userId, null);
        }
        // Downgrade to another plan --> Bill new plan at end of billing period.
        else
        {
            await stripeService.DowngradeUserSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!,
                currentSubscription.SubscriptionPlan!.PriceId, requestedSubscription.PriceId);
            await AuditTransactionHistory("Plan downgrade Requested", userId, requestedSubscription.PriceId);
        }
    }

    private async Task UpgradeSubscription(SubscriptionPlan requestedSubscription, UserSubscription currentSubscription,
        string userId)
    {
        await stripeService.UpgradeUserSubscriptionAsync(currentSubscription.SubscriptionId!,
            requestedSubscription.PriceId);
        await AuditTransactionHistory("Plan upgrade Requested", userId, requestedSubscription.PriceId);
    }

    private async Task<UserSubscription> HandleStripeSubscription(UserSubscription currentSubscription, string userId,
        string email, SubscriptionRequestDto subscriptionRequest)
    {
        var customerId = await GetOrCreateCustomerAsync(userId, email, currentSubscription);

        await stripeService.AttachPaymentMethodAsync(subscriptionRequest.PaymentMethodId, customerId);
        await stripeService.SetDefaultPaymentMethodAsync(subscriptionRequest.PaymentMethodId, customerId);

        var createdSubscriptionId = await stripeService.CreateSubscriptionAsync(customerId,
            subscriptionRequest.PriceId, userId, subscriptionRequest.PlanName);

        return await UpdateUserSubscriptionWithDetails(currentSubscription, customerId, createdSubscriptionId);
    }

    private async Task<UserSubscription?> ValidateUserSubscriptionAsync(string userId)
    {
        var currentSubscription = await FindUserSubscriptionAsync(userId);

        if (currentSubscription is null)
        {
            throw new GeneralException("Something went wrong", HttpStatusCode.BadRequest);
        }

        if (currentSubscription is not null && currentSubscription.SubscriptionPlanId != (long)SubscriptionPlanId.Basic)
        {
            throw new ValidationException("User is already subscribed", HttpStatusCode.BadRequest);
        }

        return currentSubscription;
    }

    private async Task<string> GetOrCreateCustomerAsync(string userId, string email,
        UserSubscription currentSubscription)
    {
        if (currentSubscription.CustomerId is not null)
        {
            return currentSubscription.CustomerId;
        }

        return await stripeService.CreateCustomerAsync(userId, email);
    }

    private async Task<UserSubscription?> FindUserSubscriptionAsync(string userId)
    {
        return await userRepository.FindUserSubscriptionByUserIdAsync(userId);
    }

    private async Task<UserSubscription> UpdateUserSubscriptionWithDetails(UserSubscription currentSubscription,
        string customerId, string subscriptionId)
    {
        currentSubscription.CustomerId = customerId;
        currentSubscription.SubscriptionId = subscriptionId;

        return await userRepository.SaveUserSubscriptionAsync(currentSubscription);
    }

    private async Task AuditTransactionHistory(string description, string userId, string? planPriceId)
    {
        var userTransactionHistory = new UserTransactionHistory
        {
            Description = "Plan upgrade Requested",
            UserId = userId,
            PlanPriceId = planPriceId
        };
        await userRepository.SaveUserTransactionHistoryAsync(userTransactionHistory);
    }
}