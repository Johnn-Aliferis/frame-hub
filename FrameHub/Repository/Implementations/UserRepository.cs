using FrameHub.ContextConfiguration;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly DbSet<User> _user = context.Set<User>();
    private readonly DbSet<UserInfo> _userInfo = context.Set<UserInfo>();
    private readonly DbSet<UserCredential> _userCredential = context.Set<UserCredential>();
    private readonly DbSet<UserSubscription> _userSubscription = context.Set<UserSubscription>();
    
    public async Task<User?> FindUserByIdAsync(long userId)
    {
      return await _user.FindActiveByIdAsync(userId);
    }
    
    public async Task<UserCredential?> FindUserCredentialByEmailAsync(string email)
    {
        return await _userCredential.FirstOrDefaultAsync(e => e.Email == email && e.Status);
    }

    public async Task<UserInfo?> FindUserInfoByUserIdAsync(long userId)
    {
        return await _userInfo.FindActiveByIdAsync(userId);
    }

    public async Task<UserCredential?> FindUserCredentialByUserIdAsync(long userId)
    {
        return await _userCredential.FindActiveByIdAsync(userId);
    }

    public async Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(long userId)
    {
        return await _userSubscription.FindActiveByIdAsync(userId);
    }
    
    public async Task<User> SaveUserAsync(User user)
    { 
        await _user.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }
}