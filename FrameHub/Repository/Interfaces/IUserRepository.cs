using FrameHub.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Repository.Interfaces;

public interface IUserRepository
{
    Task<IdentityUser?> FindUserByIdAsync(string userId);
    Task<IdentityUser?> FindUserByEmailAsync(string email);
    Task<IdentityUser> SaveUserAsync(IdentityUser user);
    Task<UserInfo?> FindUserInfoByUserIdAsync(string userId);
    Task<UserInfo> SaveUserInfoAsync(UserInfo userInfo);
    Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(string userId);
    Task<UserSubscription> SaveUserSubscriptionAsync(UserSubscription userSubscription);
}