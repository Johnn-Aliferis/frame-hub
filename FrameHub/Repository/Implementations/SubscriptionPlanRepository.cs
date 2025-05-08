using FrameHub.ContextConfiguration;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

public class SubscriptionPlanRepository(AppDbContext context): ISubscriptionPlanRepository
{
    private readonly DbSet<SubscriptionPlan> _subscriptionPlan = context.Set<SubscriptionPlan>();
    
    public async Task<SubscriptionPlan?> FindSubscriptionPlanByIdAsync(long planId)
    {
        return await _subscriptionPlan.FindActiveByIdAsync(planId);
    }
}