using System.Net;
using FrameHub.ContextConfiguration;
using FrameHub.Exceptions;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger) : IUserRepository
{
    private readonly DbSet<ApplicationUser> _user = context.Set<ApplicationUser>();
    private readonly DbSet<UserInfo> _userInfo = context.Set<UserInfo>();
    private readonly DbSet<UserSubscription> _userSubscription = context.Set<UserSubscription>();
    private readonly DbSet<UserTransactionHistory> _userTransactionHistory = context.Set<UserTransactionHistory>();

    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
        return await _user.FindAsync(userId);
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await _user
            .Where(u => u.Email == email && u.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<ApplicationUser> SaveUserAsync(ApplicationUser user)
    {
        await _user.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<UserInfo?> FindUserInfoByUserIdAsync(string userId)
    {
        return await _userInfo
            .Where(ui => ui.UserId == userId && ui.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<UserInfo> SaveUserInfoAsync(UserInfo userInfo)
    {
        await _userInfo.AddAsync(userInfo);
        await context.SaveChangesAsync();
        return userInfo;
    }

    public async Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(string userId)
    {
        return await _userSubscription
            .Where(us => us.UserId == userId && us.Status)
            .FirstOrDefaultAsync();
    }
    
    public async Task<long?> FindUserSubscriptionIdByUserIdAsync(string userId)
    {
        var subscription = await _userSubscription
            .Where(us => us.UserId == userId && us.Status)
            .FirstOrDefaultAsync();

        return subscription?.Id;
    }
    
    public async Task<UserSubscription?> FindUserSubscriptionByIdAsync(long userSubscriptionId)
    {
        return await _userSubscription
            .Where(us => us.Id == userSubscriptionId && us.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<UserSubscription?> FindUserSubscriptionByUserEmailAsync(string email)
    {
        return await _userSubscription
            .Where(us => us.User!.Email == email && us.Status)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserSubscription?> FindUserSubscriptionByCustomerIdAsync(string customerId)
    {
        return await _userSubscription
            .Where(us => us.CustomerId == customerId && us.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<UserSubscription> SaveUserSubscriptionAsync(UserSubscription userSubscription)
    {
        try
        {
            var existingSubscription = await _userSubscription.FindActiveByIdAsync(userSubscription.Id);

            if (existingSubscription != null)
            {
                context.Entry(existingSubscription).CurrentValues.SetValues(userSubscription);
            }
            else
            {
                await _userSubscription.AddAsync(userSubscription);
            }

            await context.SaveChangesAsync();
            return userSubscription;
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred with message : {}", ex.ToString());
            throw new GeneralException("An unexpected error occurred during subscription save. ",
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task SaveUserTransactionHistoryAsync(
        UserTransactionHistory userTransactionHistory)
    {
        await _userTransactionHistory.AddAsync(userTransactionHistory);
        await context.SaveChangesAsync();
    }
}