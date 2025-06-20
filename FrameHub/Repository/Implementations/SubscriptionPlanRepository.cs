using System.Net;
using FrameHub.ContextConfiguration;
using FrameHub.Exceptions;
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
    public async Task<SubscriptionPlan?> FindSubscriptionPlanByPriceIdAsync(string priceId)
    {
        return await _subscriptionPlan
            .Where(plan => plan.PriceId == priceId && plan.Status)
            .FirstOrDefaultAsync();
    }
    public async Task<int> FindSubscriptionPlanMaxUploadsByIdAsync(long planId)
    {
      var subscriptionPlan =  await _subscriptionPlan.FirstOrDefaultAsync(e => e.Id == planId && e.Status);
      
      if (subscriptionPlan == null)
      {
          throw new GeneralException("Subscription plan not found or inactive.", HttpStatusCode.BadRequest);
      }

      return subscriptionPlan.MaxUploads;
    }
}