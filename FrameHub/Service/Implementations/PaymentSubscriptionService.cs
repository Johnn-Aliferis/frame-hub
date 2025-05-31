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
    SubscriptionPlanRepository subscriptionPlanRepository)
    : IPaymentSubscriptionService
{
    public async Task<UserSubscriptionDto> CreateSubscriptionAsync(string userId, string email,
        SubscriptionRequestDto subscriptionRequest)
    {
        // todo : REMOVE BELOW LINE FOR PRODUCTION , RIGHT NOW ONLY FOR TEST
        subscriptionRequest.PaymentMethodId = await stripeService.CreateTestCardPaymentMethodAsync("tok_visa");
        var currentSubscription = await ValidateUserSubscriptionAsync(userId);

        var result = await HandleStripeSubscription(currentSubscription!, userId, email, subscriptionRequest, true);
        return mapper.Map<UserSubscriptionDto>(result);
    }

    public async Task<UserSubscriptionDto> UpdateSubscriptionAsync(string userId, string email,
        SubscriptionRequestDto subscriptionRequest)
    {
        // 1) Find user current sub and requested. 
        var currentSubscription = await userRepository.FindUserSubscriptionByUserEmailAsync(email);
        var requestedSubscription =
            await subscriptionPlanRepository.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId);

        if (requestedSubscription is null)
        {
            throw new ValidationException("A subscription with the requested price does not exist.",
                HttpStatusCode.BadRequest);
        }

        if (currentSubscription!.SubscriptionPlan!.PlanOrder == requestedSubscription.PlanOrder)
        {
            // tries to update to same , trow error.
            throw new ValidationException("User is already subscribed with this plan", HttpStatusCode.BadRequest);
        }

        else if (currentSubscription.SubscriptionPlan.PlanOrder > requestedSubscription.PlanOrder)
        {
            // Downgrade subscription , handle different cases :
            // 1) Downgrade to basic plan --> delete subscription
            if (requestedSubscription.Id.Equals((long)SubscriptionPlanId.Basic))
            {
                await stripeService.DeleteUserSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!);
                // Audit transaction History .
                var userTransactionHistory = new UserTransactionHistory
                {
                    Description = "User Deletion Requested",
                    UserId = userId,
                };
                await userRepository.SaveUserTransactionHistoryAsync(userTransactionHistory);
            }
            // 2) Downgrade to another plan --> Bill new plan at end of billing period.
            else
            {
                // Schedule a payment at end of billing period with new plan details.
                // Rest is handled in webhooks.
                await stripeService.DowngradeUserSubscriptionAtEndOfBillingPeriod(currentSubscription.SubscriptionId!,
                    currentSubscription.SubscriptionPlan.PriceId,requestedSubscription.PriceId);
            }

            // 2) Downgrade to another plan --> make request to stripe and have it billed at end of current billing period.
        }
        else if (currentSubscription.SubscriptionPlan.PlanOrder < requestedSubscription.PlanOrder)
        {
            // Upgrade subscription 
            // Here we upgrade subscription and we charged immediatelly. 
            // next in webhooks , we check -> if success full , new plan starts now.
            // If not successfull --> we update again user back to his current plan with which he will be billed again at the end of curent billing period
        }
    }

    private async Task<UserSubscription> HandleStripeSubscription(UserSubscription currentSubscription, string userId,
        string email, SubscriptionRequestDto subscriptionRequest, bool isNewSubscription)
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
}