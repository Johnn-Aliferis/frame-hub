using FrameHub.ContextConfiguration;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly DbSet<ApplicationUser> _user = context.Set<ApplicationUser>();
    private readonly DbSet<UserInfo> _userInfo = context.Set<UserInfo>();
    private readonly DbSet<UserSubscription> _userSubscription = context.Set<UserSubscription>();
    
    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
      return await _user.FindAsync(userId);
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await _user
            .Where(u=> u.Email == email && u.Status)
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
    
    public async Task<UserSubscription> SaveUserSubscriptionAsync(UserSubscription userSubscription)
    {
        await _userSubscription.AddAsync(userSubscription);
        await context.SaveChangesAsync();
        return userSubscription;
    }
}