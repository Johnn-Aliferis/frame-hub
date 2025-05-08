using FrameHub.Model.Entities;

namespace FrameHub.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> FindUserByIdAsync(long userId);
    Task<UserInfo?> FindUserInfoByUserIdAsync(long userId);
    Task<UserCredential?> FindUserCredentialByEmailAsync(string email);
    Task<UserCredential?> FindUserCredentialByUserIdAsync(long userId);
    Task<UserSubscription?> FindUserSubscriptionByUserIdAsync(long userId);
    
    Task<User> SaveUserAsync(User user);
}