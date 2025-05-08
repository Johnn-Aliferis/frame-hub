using FrameHub.Model.Entities;

namespace FrameHub.Repository.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> FindSubscriptionPlanByIdAsync(long planId);
}