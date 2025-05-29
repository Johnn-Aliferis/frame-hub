using Stripe;

namespace FrameHub.Service.Interfaces;

public interface IStripeConsumerService
{
    Task HandleMessage(Event stripeEvent);
}