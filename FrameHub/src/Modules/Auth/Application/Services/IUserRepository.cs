using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Subscriptions.Domain.Entities;

namespace FrameHub.Modules.Auth.Application.Services;

public interface IUserRepository
{
    Task<ApplicationUser?> FindUserByIdAsync(string userId);
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<ApplicationUser> SaveUserAsync(ApplicationUser user);
    Task<UserInfo?> FindUserInfoByUserIdAsync(string userId);
    Task<UserInfo> SaveUserInfoAsync(UserInfo userInfo);
    Task<long?> FindUserSubscriptionIdByUserIdAsync(string userId);
    Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(string userId);
    Task<UserSubscription?> FindUserSubscriptionByIdAsync(long userSubscriptionId);
    Task<UserSubscription?> FindUserSubscriptionByUserEmailAsync(string email);
    Task<UserSubscription?> FindUserSubscriptionByCustomerIdAsync(string customerId);
    Task<UserSubscription> SaveUserSubscriptionAsync(UserSubscription userSubscription);
    Task SaveUserTransactionHistoryAsync(UserTransactionHistory userTransactionHistory);
}