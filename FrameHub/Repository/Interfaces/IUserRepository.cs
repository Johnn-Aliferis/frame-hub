using FrameHub.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Repository.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindUserByIdAsync(string userId);
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<ApplicationUser> SaveUserAsync(ApplicationUser user);
    Task<UserInfo?> FindUserInfoByUserIdAsync(string userId);
    Task<UserInfo> SaveUserInfoAsync(UserInfo userInfo);
    Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(string userId);
    Task<UserSubscription?> FindUserSubscriptionByUserEmailAsync(string email);
    Task<UserSubscription?> FindUserSubscriptionByCustomerIdAsync(string customerId);
    Task<UserSubscription> SaveUserSubscriptionAsync(UserSubscription userSubscription);
    Task SaveUserTransactionHistoryAsync(UserTransactionHistory userTransactionHistory);
}