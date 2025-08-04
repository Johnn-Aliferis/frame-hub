using Stripe;

namespace FrameHub.Modules.Subscriptions.Application.Service;

public interface IStripeConsumerService
{
    Task HandleMessage(Event stripeEvent);
}