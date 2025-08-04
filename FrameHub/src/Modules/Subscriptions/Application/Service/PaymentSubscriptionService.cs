using System.Net;
using AutoMapper;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Shared.Application.Exception;
using FrameHub.Modules.Subscriptions.API.DTO;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using FrameHub.Modules.Subscriptions.Domain.Enum;

namespace FrameHub.Modules.Subscriptions.Application.Service;

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
        subscriptionRequest.PaymentMethodId = await stripeService.CreateTestCardPaymentMethodAsync("tok_visa");
        var currentSubscription = await ValidateUserSubscriptionAsync(userId);

        var result = await HandleStripeSubscription(currentSubscription!, userId, email, subscriptionRequest);
        return mapper.Map<UserSubscriptionDto>(result);
    }

    public async Task DeleteSubscriptionAsync(long userSubscriptionId, string userId)
    {
        await ValidateUserSubscriptionChangeAsync(userSubscriptionId, userId);
        
        var subscriptionId = await userRepository.FindUserSubscriptionByIdAsync(userSubscriptionId);
        if (subscriptionId is null)
        {
            throw new ValidationException("Cannot find subscription", HttpStatusCode.BadRequest);
        }

        await stripeService.DeleteUserSubscriptionAtEndOfBillingPeriod(subscriptionId.SubscriptionId!);
        await AuditTransactionHistory("User Deletion Requested", userId, null);
    }

    public async Task UpdateSubscriptionAsync(long userSubscriptionId, string userId, string email,
        SubscriptionRequestDto subscriptionRequest)
    {
        await ValidateUserSubscriptionChangeAsync(userSubscriptionId, userId);
        
        // Future enhancement -> re-attach or update payment method and default method.

        var currentSubscription = await userRepository.FindUserSubscriptionByIdAsync(userSubscriptionId);
        
        var requestedSubscription =
            await subscriptionPlanRepository.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId);

        if (requestedSubscription is null)
        {
            throw new ValidationException("A subscription with the requested priceId does not exist.",
                HttpStatusCode.BadRequest);
        }

        if (currentSubscription!.SubscriptionPlan!.PlanOrder == requestedSubscription.PlanOrder)
        {
            await ApplySameSubscription(requestedSubscription, currentSubscription, userId);
        }

        if (currentSubscription.SubscriptionPlan.PlanOrder > requestedSubscription.PlanOrder)
        {
            await DowngradeSubscription(requestedSubscription, currentSubscription, userId);
        }
        else if (currentSubscription.SubscriptionPlan.PlanOrder < requestedSubscription.PlanOrder)
        {
            await UpgradeSubscription(requestedSubscription, currentSubscription, userId);
        }
    }

    private async Task ApplySameSubscription(SubscriptionPlan requestedSubscription,
        UserSubscription currentSubscription, string userId)
    {
        await stripeService.ScheduleNewSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!,
            requestedSubscription.PriceId);
        await AuditTransactionHistory("Plan downgrade Requested", userId, requestedSubscription.PriceId);
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
            await stripeService.ScheduleNewSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!,
                requestedSubscription.PriceId);
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
    
    private async Task ValidateUserSubscriptionChangeAsync(long subscriptionIdReceived, string userId)
    {
        var userSubscriptionId = await userRepository.FindUserSubscriptionIdByUserIdAsync(userId);

        if (userSubscriptionId is null)
        {
            throw new GeneralException("Something went wrong", HttpStatusCode.BadRequest);
        }

        if (userSubscriptionId is not null && userSubscriptionId != subscriptionIdReceived)
        {
            throw new ValidationException("The provided subscription does not belong to the current user", HttpStatusCode.BadRequest);
        }
        
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
            Description = description,
            UserId = userId,
            PlanPriceId = planPriceId
        };
        await userRepository.SaveUserTransactionHistoryAsync(userTransactionHistory);
    }
}