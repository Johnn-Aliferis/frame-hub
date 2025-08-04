using System.Net;
using AutoMapper;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Shared.Application.Interface;
using FrameHub.Modules.Subscriptions.Application.Exception;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;

namespace FrameHub.Tests.Modules.Subscriptions.Application.Service;

public class StripeConsumerServiceTests
{
    private readonly Mock<IWebhookEventRepository> _webhookEventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _iMapperMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly Mock<ILogger<StripeConsumerService>> _loggerMock;
    private readonly StripeConsumerService _service;

    public StripeConsumerServiceTests()
    {
        _webhookEventRepositoryMock = new Mock<IWebhookEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _iMapperMock = new Mock<IMapper>();
        _stripeServiceMock = new Mock<IStripeService>();
        _loggerMock = new Mock<ILogger<StripeConsumerService>>();
        _service = new StripeConsumerService(
            _webhookEventRepositoryMock.Object,
            _iMapperMock.Object,
            _userRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _stripeServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleMessage_Failure_Empty_Event()
    {
        Event? receivedEvent = null;
        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Message received was null", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task HandleMessage_Success_Received_Duplicate_Event()
    {
        var receivedEvent = new Event { Id = "test-id" };
        var webhookEvent = new WebhookEvent
            { EventId = "event-id", EventType = "event-type", RawPayload = "raw-payload" };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync(webhookEvent);

        await _service.HandleMessage(receivedEvent);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Failure_Invoice_Received_Null()
    {
        Invoice invoice = null!;
        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Invoice received is null", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }

    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Failure_Invoice_Received_Empty_Email()
    {
        var invoice = new Invoice();
        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Message does not contain user information", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }

    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Failure_Invoice_Received_Empty_Billing_Reason()
    {
        var invoice = new Invoice
        {
            CustomerEmail = "test-customer-email"
        };
        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Message does not contain Billing Reason", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }


    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Failure_User_Not_Associated_With_Given_Email()
    {
        var invoice = new Invoice
        {
            CustomerEmail = "test-customer-email",
            BillingReason = "subscription_create"
        };
        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserSubscription?)null);

        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Cannot find user subscription associated with given email", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }

    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Failure_Requested_Plan_Not_Found()
    {
        const string price = "test-price";
        var priceDetails = new InvoiceLineItemPricingPriceDetails { Price = price };
        var invoiceLinePricing = new InvoiceLineItemPricing { PriceDetails = priceDetails };
        var invoiceLineItem = new InvoiceLineItem
        {
            Pricing = invoiceLinePricing
        };

        var list = new StripeList<InvoiceLineItem>
        {
            Data = [invoiceLineItem]
        };

        var invoice = new Invoice
        {
            CustomerEmail = "test-customer-email",
            BillingReason = "subscription_create",
            Lines = list
        };

        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserSubscription { UserId = "test-user-id" });

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(price))
            .ReturnsAsync((SubscriptionPlan?)null);

        var exception = await Assert.ThrowsAsync<StripeConsumerException>(async () =>
            await _service.HandleMessage(receivedEvent)
        );

        Assert.Equal("Cannot find requested plan in DB", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }


    [Fact]
    public async Task HandleMessage_PaymentInvoiceSuccess_Success()
    {
        const string price = "test-price";
        var priceDetails = new InvoiceLineItemPricingPriceDetails { Price = price };
        var invoiceLinePricing = new InvoiceLineItemPricing { PriceDetails = priceDetails };
        var invoiceLineItemPeriod = new InvoiceLineItemPeriod
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(30)
        };
        var invoiceLineItemParentSubscriptionDetails = new InvoiceLineItemParentSubscriptionItemDetails
        {
            Subscription = "Subscription"
        };
        var invoiceLineItemParent = new InvoiceLineItemParent
        {
            SubscriptionItemDetails = invoiceLineItemParentSubscriptionDetails
        };
        var invoiceLineItem = new InvoiceLineItem
        {
            Pricing = invoiceLinePricing,
            Period = invoiceLineItemPeriod,
            Parent = invoiceLineItemParent
        };

        var list = new StripeList<InvoiceLineItem>
        {
            Data = [invoiceLineItem]
        };

        var invoice = new Invoice
        {
            CustomerEmail = "test-customer-email",
            BillingReason = "subscription_create",
            Lines = list
        };

        var receivedEventData = new EventData
        {
            Object = invoice
        };

        var receivedEvent = new Event
        {
            Id = "test-id", Type = "invoice.payment_succeeded", Data = receivedEventData
        };

        var persistedWebhookEvent = new WebhookEvent
        {
            EventId = "event-id",
            EventType = "event-type",
            RawPayload = "raw-payload"
        };

        _webhookEventRepositoryMock.Setup(x => x.FindWebhookEventByEventIdAsync(receivedEvent.Id))
            .ReturnsAsync((WebhookEvent?)null);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        _iMapperMock.Setup(mapper => mapper.Map<WebhookEvent>(It.IsAny<Event>())).Returns(persistedWebhookEvent);

        _webhookEventRepositoryMock.Setup(x => x.PersistWebhookDataAsync(persistedWebhookEvent))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserSubscription { UserId = "test-user-id" });

        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanByPriceIdAsync(price)).ReturnsAsync(
            new SubscriptionPlan
            {
                Code = "code",
                Name = "name",
                PriceId = "priceId",
                ProductId = "productId",
                PlanOrder = 2
            });
        
        _userRepositoryMock.Setup(x => x.SaveUserSubscriptionAsync(It.IsAny<UserSubscription>()))
            .ReturnsAsync(new UserSubscription { UserId = "test-user-id" });
        
        _userRepositoryMock.Setup(x=>x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>())).Returns(Task.CompletedTask);

        await _service.HandleMessage(receivedEvent);

        _userRepositoryMock.Verify(x => x.SaveUserSubscriptionAsync(It.IsAny<UserSubscription>()), Times.Once);
        
        _userRepositoryMock.Verify(x=>x.SaveUserTransactionHistoryAsync(It.IsAny<UserTransactionHistory>()), Times.Once);
    }
}