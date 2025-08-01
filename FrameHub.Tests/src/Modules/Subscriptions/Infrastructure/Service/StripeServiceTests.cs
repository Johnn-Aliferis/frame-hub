using FrameHub.Modules.Subscriptions.Infrastructure.Service;
using Moq;
using Stripe;

namespace FrameHub.Tests.Modules.Subscriptions.Infrastructure.Service;

public class StripeServiceTests
{
    private readonly Mock<CustomerService> _customerServiceMock;
    private readonly Mock<PaymentMethodService> _paymentMethodServiceMock;
    private readonly Mock<SubscriptionService> _subscriptionServiceMock;
    private readonly Mock<InvoiceService> _invoiceServiceMock;
    private readonly StripeService _service;


    public StripeServiceTests()
    {
        _customerServiceMock = new Mock<CustomerService>(MockBehavior.Strict);
        _paymentMethodServiceMock = new Mock<PaymentMethodService>(MockBehavior.Strict);
        _subscriptionServiceMock = new Mock<SubscriptionService>(MockBehavior.Strict);
        _invoiceServiceMock = new Mock<InvoiceService>(MockBehavior.Strict);
        _service = new StripeService(_customerServiceMock.Object, _paymentMethodServiceMock.Object,
            _subscriptionServiceMock.Object, _invoiceServiceMock.Object);
    }

    [Fact]
    public async Task CreateCustomerAsync_Success()
    {
        const string userId = "userId";
        const string email = "email";

        _customerServiceMock.Setup(x => x.CreateAsync(It.IsAny<CustomerCreateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = userId });

        var result = await _service.CreateCustomerAsync(userId, email);


        //Assertions 
        Assert.NotNull(result);
        Assert.Equal(userId, result);
    }
    
    [Fact]
    public async Task CreateCustomerAsync_Failure()
    {
        const string userId = "userId";
        const string email = "email";

        _customerServiceMock.Setup(x => x.CreateAsync(It.IsAny<CustomerCreateOptions>(),
                It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException());

        await Assert.ThrowsAsync<StripeException>(() => _service.CreateCustomerAsync(userId, email));
    }

    [Fact]
    public async Task Attach_PaymentMethodAsync_Success_Existing()
    {
        const string customerId = "customerId";
        const string paymentMethodId = "paymentMethodId";

        _paymentMethodServiceMock.Setup(x => x.GetAsync(paymentMethodId, It.IsAny<PaymentMethodGetOptions>(),
                It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentMethod { CustomerId = customerId });

        await _service.AttachPaymentMethodAsync(paymentMethodId, customerId);

        _paymentMethodServiceMock.Verify(
            x => x.AttachAsync(It.IsAny<string>(), It.IsAny<PaymentMethodAttachOptions>(), It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Attach_PaymentMethodAsync_Success_Non_Existing()
    {
        const string customerId = "customerId";
        const string paymentMethodId = "paymentMethodId";

        _paymentMethodServiceMock.Setup(x => x.GetAsync(paymentMethodId, It.IsAny<PaymentMethodGetOptions>(),
                It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentMethod { CustomerId = null });

        _paymentMethodServiceMock
            .Setup(x => x.AttachAsync(paymentMethodId,
                It.Is<PaymentMethodAttachOptions>(opt => opt.Customer == customerId),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentMethod());

        await _service.AttachPaymentMethodAsync(paymentMethodId, customerId);

        _paymentMethodServiceMock.Verify(
            x => x.AttachAsync(It.IsAny<string>(), It.IsAny<PaymentMethodAttachOptions>(), It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ScheduleNewSubscriptionAtEndOfBillingPeriod_Success()
    {
        const string subscriptionId = "subscriptionId";
        const string newPlanPriceId = "newPlanPriceId";
        const string subscriptionItemId = "subscriptionItemId";

        var subscription = new Subscription
        {
            Id = subscriptionId,
            Items = new StripeList<SubscriptionItem>
            {
                Data = new List<SubscriptionItem>
                {
                    new SubscriptionItem { Id = subscriptionItemId }
                }
            }
        };

        _subscriptionServiceMock.Setup(x => x.GetAsync(subscriptionId, It.IsAny<SubscriptionGetOptions>(),
                It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _subscriptionServiceMock
            .Setup(x => x.UpdateAsync(
                subscriptionId,
                It.Is<SubscriptionUpdateOptions>(options =>
                    options.Items.Count == 1 &&
                    options.Items[0].Id == subscriptionItemId &&
                    options.Items[0].Price == newPlanPriceId &&
                    options.Items[0].Quantity == 1 &&
                    options.CancelAtPeriodEnd == false &&
                    options.ProrationBehavior == "none" &&
                    options.BillingCycleAnchor == SubscriptionBillingCycleAnchor.Unchanged),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        await _service.ScheduleNewSubscriptionAtEndOfBillingPeriod(subscriptionId, newPlanPriceId);

        _subscriptionServiceMock.Verify(x => x.UpdateAsync(
                subscriptionId,
                It.IsAny<SubscriptionUpdateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpgradeUserSubscriptionAsync_Success()
    {
        const string subscriptionId = "subscriptionId";
        const string newPriceId = "newPriceId";
        const string subscriptionItemId = "subscriptionItemId";
        const string customerId = "customerId";

        var currentSubscription = new Subscription
        {
            Id = subscriptionId,
            CustomerId = customerId,
            Items = new StripeList<SubscriptionItem>
            {
                Data = new List<SubscriptionItem>
                {
                    new SubscriptionItem { Id = subscriptionItemId }
                }
            }
        };

        _subscriptionServiceMock
            .Setup(x => x.GetAsync(
                subscriptionId,
                It.IsAny<SubscriptionGetOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentSubscription);

        _subscriptionServiceMock
            .Setup(x => x.UpdateAsync(
                subscriptionId,
                It.Is<SubscriptionUpdateOptions>(opts =>
                    opts.Items.Count == 1 &&
                    opts.Items[0].Id == subscriptionItemId &&
                    opts.Items[0].Price == newPriceId &&
                    opts.ProrationBehavior == "none" &&
                    opts.CollectionMethod == "charge_automatically" &&
                    opts.BillingCycleAnchor == SubscriptionBillingCycleAnchor.Now),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentSubscription);

        _invoiceServiceMock.Setup(x => x.CreateAsync(
                It.Is<InvoiceCreateOptions>(opts =>
                    opts.Customer == customerId &&
                    opts.Subscription == subscriptionId &&
                    opts.AutoAdvance == true),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice());

        await _service.UpgradeUserSubscriptionAsync(subscriptionId, newPriceId);

        _subscriptionServiceMock.Verify(x => x.UpdateAsync(
                subscriptionId,
                It.IsAny<SubscriptionUpdateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _invoiceServiceMock.Verify(x => x.CreateAsync(
                It.IsAny<InvoiceCreateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RevertUserSubscriptionAsync_Success()
    {
        const string subscriptionId = "subscriptionId";
        const string previousPriceId = "previousPriceId";
        const string subscriptionItemId = "subscriptionItemId";
        var trialEndDate = new DateTime(2025, 12, 31);

        var currentSubscription = new Subscription
        {
            Id = subscriptionId,
            Items = new StripeList<SubscriptionItem>
            {
                Data = new List<SubscriptionItem>
                {
                    new SubscriptionItem { Id = subscriptionItemId }
                }
            }
        };

        _subscriptionServiceMock
            .Setup(x => x.GetAsync(
                subscriptionId,
                It.IsAny<SubscriptionGetOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentSubscription);

        _subscriptionServiceMock
            .Setup(x => x.UpdateAsync(
                subscriptionId,
                It.Is<SubscriptionUpdateOptions>(opts =>
                    opts.Items.Count == 1 &&
                    opts.Items[0].Id == subscriptionItemId &&
                    opts.Items[0].Price == previousPriceId &&
                    opts.ProrationBehavior == "none" &&
                    opts.PaymentBehavior == "allow_incomplete" &&
                    opts.TrialEnd == trialEndDate),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentSubscription);

        await _service.RevertUserSubscriptionAsync(subscriptionId, previousPriceId, trialEndDate);

        _subscriptionServiceMock.Verify(x => x.UpdateAsync(
                subscriptionId,
                It.IsAny<SubscriptionUpdateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SetDefaultPaymentMethodAsync_Success()
    {
        const string customerId = "customerId";
        const string paymentMethodId = "paymentMethodId";

        _customerServiceMock.Setup(x => x.UpdateAsync(
                customerId,
                It.Is<CustomerUpdateOptions>(opts =>
                    opts.InvoiceSettings != null &&
                    opts.InvoiceSettings.DefaultPaymentMethod == paymentMethodId),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer());

        await _service.SetDefaultPaymentMethodAsync(paymentMethodId, customerId);

        _customerServiceMock.Verify(x => x.UpdateAsync(
                customerId,
                It.Is<CustomerUpdateOptions>(opts =>
                    opts.InvoiceSettings.DefaultPaymentMethod == paymentMethodId),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_Success()
    {
        const string customerId = "customerId";
        const string priceId = "priceId";
        const string userId = "userId";
        const string planName = "planName";
        const string returnedSubscriptionId = "returnedSubscriptionId";
        
        _subscriptionServiceMock
            .Setup(x => x.CreateAsync(
                It.Is<SubscriptionCreateOptions>(opts =>
                    opts.Customer == customerId &&
                    opts.Items.Count == 1 &&
                    opts.Items[0].Price == priceId &&
                    opts.Metadata["userId"] == userId &&
                    opts.Metadata["plan"] == planName),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription { Id = returnedSubscriptionId });
        
        var result = await _service.CreateSubscriptionAsync(customerId, priceId, userId, planName);
        
        Assert.Equal(returnedSubscriptionId, result);

        _subscriptionServiceMock.Verify(x => x.CreateAsync(
                It.IsAny<SubscriptionCreateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTestCardPaymentMethodAsync_Sucess()
    {
        const string testToken = "testToken";
        const string expectedPaymentMethodId = "expectedPaymentMethodId";
        
        _paymentMethodServiceMock.
            Setup(x => x.CreateAsync(
                It.Is<PaymentMethodCreateOptions>(opts =>
                    opts.Type == "card" &&
                    opts.Card != null &&
                    opts.Card.Token == testToken),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentMethod { Id = expectedPaymentMethodId });
        
        var result = await _service.CreateTestCardPaymentMethodAsync(testToken);
        Assert.Equal(expectedPaymentMethodId, result);

        _paymentMethodServiceMock.Verify(x => x.CreateAsync(
                It.IsAny<PaymentMethodCreateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
}