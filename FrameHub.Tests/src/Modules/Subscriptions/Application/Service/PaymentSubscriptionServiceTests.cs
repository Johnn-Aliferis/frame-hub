using System.Net;
using AutoMapper;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Shared.Application.Exception;
using FrameHub.Modules.Subscriptions.API.DTO;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using Moq;

namespace FrameHub.Tests.Modules.Subscriptions.Application.Service;

public class PaymentSubscriptionServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly PaymentSubscriptionService _service;


    public PaymentSubscriptionServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _stripeServiceMock = new Mock<IStripeService>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _service = new PaymentSubscriptionService(_userRepositoryMock.Object, _mapperMock.Object,
            _stripeServiceMock.Object, _subscriptionPlanRepositoryMock.Object);
    }


     /**** CreateSubscriptionPlanAsync tests ****/ 
    [Fact]
    public async Task CreateSubscriptionPlanAsync_Failure_Not_Found()
    {
        const string cardPaymentMethodResponse = "responseCardMethod";
        const string userId = "userId";
        const string email = "user-email";

        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _stripeServiceMock.Setup(x => x.CreateTestCardPaymentMethodAsync(It.IsAny<string>()))
            .ReturnsAsync(cardPaymentMethodResponse);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync((UserSubscription?)null);

        var exception = await Assert.ThrowsAsync<GeneralException>(async () =>
            await _service.CreateSubscriptionAsync(userId, email, subscriptionRequest)
        );

        // Assertions
        Assert.Equal("Something went wrong", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task CreateSubscriptionPlanAsync_Failure_Wrong_SubscriptionPlan()
    {
        const string cardPaymentMethodResponse = "responseCardMethod";
        const string userId = "userId";
        const string email = "user-email";

        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _stripeServiceMock.Setup(x => x.CreateTestCardPaymentMethodAsync(It.IsAny<string>()))
            .ReturnsAsync(cardPaymentMethodResponse);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync(new UserSubscription { UserId = userId, SubscriptionPlanId = 2 });

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.CreateSubscriptionAsync(userId, email, subscriptionRequest)
        );

        // Assertions
        Assert.Equal("User is already subscribed", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task CreateSubscriptionPlanAsync_Success()
    {
        const string cardPaymentMethodResponse = "responseCardMethod";
        const string userId = "userId";
        const string email = "user-email";
        const string createdStripeCustomerId = "stripe-customer-id";
        const string createdSubscriptionId = "created-subscription-id";

        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _stripeServiceMock.Setup(x => x.CreateTestCardPaymentMethodAsync(It.IsAny<string>()))
            .ReturnsAsync(cardPaymentMethodResponse);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync(new UserSubscription { UserId = userId, SubscriptionPlanId = 1 });

        _stripeServiceMock.Setup(x => x.CreateCustomerAsync(userId, email))
            .ReturnsAsync(createdStripeCustomerId);

        _stripeServiceMock.Setup(x =>
                x.AttachPaymentMethodAsync(subscriptionRequest.PaymentMethodId, createdStripeCustomerId))
            .Returns(Task.CompletedTask);

        _stripeServiceMock.Setup(x =>
                x.SetDefaultPaymentMethodAsync(subscriptionRequest.PaymentMethodId, createdStripeCustomerId))
            .Returns(Task.CompletedTask);

        _stripeServiceMock.Setup(x => x.CreateSubscriptionAsync(createdStripeCustomerId, subscriptionRequest.PriceId,
                userId, subscriptionRequest.PlanName))
            .ReturnsAsync(createdSubscriptionId);

        _userRepositoryMock.Setup(x => x.SaveUserSubscriptionAsync(It.IsAny<UserSubscription>()))
            .ReturnsAsync(new UserSubscription
            {
                UserId = userId,
                SubscriptionPlanId = 1,
                CustomerId = createdStripeCustomerId,
                SubscriptionId = createdSubscriptionId
            });
        
        _mapperMock.Setup(mapper => mapper.Map<UserSubscriptionDto>(It.IsAny<UserSubscription>()))
            .Returns(new UserSubscriptionDto
        {
            UserId = userId,
            CustomerId = createdStripeCustomerId,
            SubscriptionId = createdSubscriptionId
        });

        var result = await _service.CreateSubscriptionAsync(userId, email, subscriptionRequest);

        
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(createdStripeCustomerId, result.CustomerId);
        Assert.Equal(createdSubscriptionId, result.SubscriptionId);
    }
}