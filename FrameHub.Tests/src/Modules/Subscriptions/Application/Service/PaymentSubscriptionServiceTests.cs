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


    /**** DeleteSubscriptionAsync tests ****/

    [Fact]
    public async Task DeleteSubscriptionAsync_Failure()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync((long?)null);

        var exception = await Assert.ThrowsAsync<GeneralException>(async () =>
            await _service.DeleteSubscriptionAsync(userSubscriptionId, userId)
        );

        // Assertions
        Assert.Equal("Something went wrong", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_Failure_Subscription_Not_Mapped_To_User()
    {
        const long subscriptionIdReceived = 1L;
        const long userSubscriptionId = 2L;
        const string userId = "user-id";

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.DeleteSubscriptionAsync(subscriptionIdReceived, userId)
        );

        // Assertions
        Assert.Equal("The provided subscription does not belong to the current user", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_Failure_Subscription_Not_Found()
    {
        const long subscriptionIdReceived = 1L;
        const string userId = "user-id";

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(subscriptionIdReceived);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(subscriptionIdReceived))
            .ReturnsAsync((UserSubscription?)null);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.DeleteSubscriptionAsync(subscriptionIdReceived, userId)
        );

        // Assertions
        Assert.Equal("Cannot find subscription", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }


    [Fact]
    public async Task DeleteSubscriptionAsync_Success()
    {
        const long subscriptionIdReceived = 1L;
        const string userId = "user-id";
        const string auditResponse = "User Deletion Requested";

        var subscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 2L,
        };

        var userTransactionHistory = new UserTransactionHistory
        {
            Description = auditResponse,
            UserId = userId,
            PlanPriceId = null
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(subscriptionIdReceived);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(subscriptionIdReceived))
            .ReturnsAsync(subscription);

        _stripeServiceMock.Setup(x =>
                x.DeleteUserSubscriptionAtEndOfBillingPeriod(subscription.SubscriptionId!))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveUserTransactionHistoryAsync(userTransactionHistory))
            .Returns(Task.CompletedTask);

        await _service.DeleteSubscriptionAsync(subscriptionIdReceived, userId);

        _userRepositoryMock.Verify(x => x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()),
            Times.Once);
    }


    /**** UpdateSubscriptionAsync tests ****/

    [Fact]
    public async Task UpdateSubscriptionAsync_Failure()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync((long?)null);

        var exception = await Assert.ThrowsAsync<GeneralException>(async () =>
            await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest)
        );

        // Assertions
        Assert.Equal("Something went wrong", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task UpdateSubscriptionAsync_Failure_Subscription_Not_Mapped_To_User()
    {
        const long userSubscriptionId = 1L;
        const long receivedUserSubscriptionId = 2L;
        const string userId = "user-id";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(receivedUserSubscriptionId);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest)
        );

        // Assertions
        Assert.Equal("The provided subscription does not belong to the current user", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }


    [Fact]
    public async Task UpdateSubscriptionAsync_Failure_Not_Existing_Price()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId))
            .ReturnsAsync((SubscriptionPlan?)null);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest)
        );

        // Assertions
        Assert.Equal("A subscription with the requested priceId does not exist.", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }


    [Fact]
    public async Task UpdateSubscriptionAsync_Apply_Same_Subscription_Success()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string auditResponse = "Plan downgrade Requested";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        var subscriptionPlan = new SubscriptionPlan
        {
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 2
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 1L,
            SubscriptionPlan = subscriptionPlan
        };
        
        var userTransactionHistory = new UserTransactionHistory
        {
            Description = auditResponse,
            UserId = userId,
            PlanPriceId = null
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(userSubscriptionId))
            .ReturnsAsync(userSubscription);

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId))
            .ReturnsAsync(subscriptionPlan);

        _stripeServiceMock.Setup(x =>
                x.ScheduleNewSubscriptionAtEndOfBillingPeriod(userSubscription.SubscriptionId!,subscriptionPlan.PriceId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveUserTransactionHistoryAsync(userTransactionHistory))
            .Returns(Task.CompletedTask);

        await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest);

        _userRepositoryMock.Verify(x => x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()),
            Times.Once);
        
        _stripeServiceMock.Verify(x => x.ScheduleNewSubscriptionAtEndOfBillingPeriod(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateSubscriptionAsync_Downgrade_Subscription_To_Basic_Success()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string auditResponse = "Plan downgrade Requested";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        var subscriptionPlan = new SubscriptionPlan
        {
            Id = 2,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 2
        };
        
        var subscriptionPlanRequested = new SubscriptionPlan
        {
            Id = 1,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 1
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 1L,
            SubscriptionPlan = subscriptionPlan
        };
        
        var userTransactionHistory = new UserTransactionHistory
        {
            Description = auditResponse,
            UserId = userId,
            PlanPriceId = null
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(userSubscriptionId))
            .ReturnsAsync(userSubscription);

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId))
            .ReturnsAsync(subscriptionPlanRequested);

        _stripeServiceMock.Setup(x =>
                x.DeleteUserSubscriptionAtEndOfBillingPeriod(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveUserTransactionHistoryAsync(userTransactionHistory))
            .Returns(Task.CompletedTask);

        await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest);

        _userRepositoryMock.Verify(x => x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()),
            Times.Once);
        
        _stripeServiceMock.Verify(x => x.DeleteUserSubscriptionAtEndOfBillingPeriod(It.IsAny<string>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateSubscriptionAsync_Downgrade_Subscription_Success()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string auditResponse = "Plan downgrade Requested";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        var subscriptionPlan = new SubscriptionPlan
        {
            Id = 3,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 3
        };
        
        var subscriptionPlanRequested = new SubscriptionPlan
        {
            Id = 2,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 2
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 1L,
            SubscriptionPlan = subscriptionPlan
        };
        
        var userTransactionHistory = new UserTransactionHistory
        {
            Description = auditResponse,
            UserId = userId,
            PlanPriceId = null
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(userSubscriptionId))
            .ReturnsAsync(userSubscription);

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId))
            .ReturnsAsync(subscriptionPlanRequested);

        _stripeServiceMock.Setup(x =>
                x.ScheduleNewSubscriptionAtEndOfBillingPeriod(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveUserTransactionHistoryAsync(userTransactionHistory))
            .Returns(Task.CompletedTask);

        await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest);

        _userRepositoryMock.Verify(x => x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()),
            Times.Once);

        _stripeServiceMock.Verify(
            x => x.ScheduleNewSubscriptionAtEndOfBillingPeriod(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    
    
    [Fact]
    public async Task UpdateSubscriptionAsync_Upgrade_Subscription_Success()
    {
        const long userSubscriptionId = 1L;
        const string userId = "user-id";
        const string auditResponse = "Plan downgrade Requested";
        const string email = "user-email";
        var subscriptionRequest = new SubscriptionRequestDto
        {
            PaymentMethodId = "methodId",
            PriceId = "stripe-price-id",
            PlanName = "stripe-plan-name",
        };

        var subscriptionPlan = new SubscriptionPlan
        {
            Id = 2,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 2
        };
        
        var subscriptionPlanRequested = new SubscriptionPlan
        {
            Id = 3,
            Code = "code",
            Name = "name",
            PriceId = "priceId",
            ProductId = "productId",
            PlanOrder = 3
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 1L,
            SubscriptionPlan = subscriptionPlan
        };
        
        var userTransactionHistory = new UserTransactionHistory
        {
            Description = auditResponse,
            UserId = userId,
            PlanPriceId = null
        };

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionIdByUserIdAsync(userId))
            .ReturnsAsync(userSubscriptionId);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByIdAsync(userSubscriptionId))
            .ReturnsAsync(userSubscription);

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(subscriptionRequest.PriceId))
            .ReturnsAsync(subscriptionPlanRequested);

        _stripeServiceMock.Setup(x =>
                x.UpgradeUserSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveUserTransactionHistoryAsync(userTransactionHistory))
            .Returns(Task.CompletedTask);

        await _service.UpdateSubscriptionAsync(userSubscriptionId, userId, email, subscriptionRequest);

        _userRepositoryMock.Verify(x => x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()),
            Times.Once);

        _stripeServiceMock.Verify(
            x => x.UpgradeUserSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}