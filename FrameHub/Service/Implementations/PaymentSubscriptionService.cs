using System.Net;
using AutoMapper;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Subscription;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class PaymentSubscriptionService(IUserRepository userRepository,IMapper mapper, IStripeService stripeService)
    : IPaymentSubscriptionService
{
    private const string PendingStatus = "Pending";

    public async Task<UserSubscriptionDto> CreateSubscriptionAsync(string userId, string email,
        CreateSubscriptionRequestDto createSubscriptionRequest)
    {
        
        // todo : REMOVE BELOW LINE FOR PRODUCTION , RIGHT NOW ONLY FOR TEST
        createSubscriptionRequest.PaymentMethodId = await stripeService.CreateTestCardPaymentMethodAsync("tok_visa");
        var currentSubscription = await ValidateUserSubscriptionAsync(userId);

        var result =  await HandleStripeSubscription(currentSubscription!, userId, email, createSubscriptionRequest);
        return mapper.Map<UserSubscriptionDto>(result);
    }

    private async Task<UserSubscription> HandleStripeSubscription(UserSubscription currentSubscription, string userId,
        string email,
        CreateSubscriptionRequestDto createSubscriptionRequest)
    {
        var customerId = await GetOrCreateCustomerAsync(userId, email, currentSubscription);
        
        await stripeService.AttachPaymentMethodAsync(createSubscriptionRequest.PaymentMethodId, customerId);
        await stripeService.SetDefaultPaymentMethodAsync(createSubscriptionRequest.PaymentMethodId, customerId);
        
        var createdSubscriptionId = await stripeService.CreateSubscriptionAsync(customerId,
            createSubscriptionRequest.PriceId, userId, createSubscriptionRequest.PlanName);
        
        return await UpdateUserSubscriptionWithDetails(currentSubscription, customerId, createdSubscriptionId,
            PendingStatus);
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

    private async Task<string> GetOrCreateCustomerAsync(string userId, string email, UserSubscription currentSubscription)
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
        string customerId, string subscriptionId, string paymentStatus)
    {
        currentSubscription.CustomerId = customerId;
        currentSubscription.SubscriptionId = subscriptionId;
        currentSubscription.PaymentStatus = paymentStatus;

        return await userRepository.SaveUserSubscriptionAsync(currentSubscription);
    }
}