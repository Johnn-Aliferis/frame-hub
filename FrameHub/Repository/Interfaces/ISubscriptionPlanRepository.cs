using FrameHub.Model.Entities;

namespace FrameHub.Repository.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> FindSubscriptionPlanByIdAsync(long planId);
    Task<SubscriptionPlan?> FindSubscriptionPlanByPriceIdAsync(string priceId);
    Task<int> FindSubscriptionPlanMaxUploadsByIdAsync(long planId);
}