using FrameHub.Modules.Subscriptions.Domain.Entities;

namespace FrameHub.Modules.Subscriptions.Application.Service;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> FindSubscriptionPlanByIdAsync(long planId);
    Task<SubscriptionPlan?> FindSubscriptionPlanByPriceIdAsync(string priceId);
    Task<int> FindSubscriptionPlanMaxUploadsByIdAsync(long planId);
}