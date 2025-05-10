using System.Net;
using AutoMapper;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Strategies;

public class DefaultRegistrationStrategy(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IMapper mapper,
    ILogger<DefaultRegistrationStrategy> logger,
    ISubscriptionPlanRepository subscriptionPlanRepository) : IRegistrationStrategy
{
    private const string LocalProvider = "local";

    public async Task<RegistrationResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var response = await HandleDefaultRegistration(registrationRequestDto);
            await unitOfWork.CommitAsync();
            return response;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "An error occurred during user registration.");
            throw new RegistrationException(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    private async Task<RegistrationResponseDto> HandleDefaultRegistration(RegistrationRequestDto registrationRequestDto)
    {
        if (await UserExists(registrationRequestDto.Email))
        {
            throw new RegistrationException($"Email {registrationRequestDto.Email} already exists.",
                HttpStatusCode.Conflict);
        }

        var user = new User { LastLogin = null };

        // User Credential
        var password = BCrypt.Net.BCrypt.HashPassword(registrationRequestDto.Password);
        var credential = new UserCredential
        {
            Email = registrationRequestDto.Email,
            PasswordHash = password,
            Provider = LocalProvider,
            ExternalId = null,
            User = user
        };

        // User Info 
        var userInfo = new UserInfo
        {
            DisplayName = registrationRequestDto.DisplayName,
            PhoneNumber = registrationRequestDto.PhoneNumber,
            User = user
        };

        // User subscription
        var subscriptionPlan =
            await subscriptionPlanRepository.FindSubscriptionPlanByIdAsync((long)SubscriptionPlanId.Basic);
        if (subscriptionPlan is null)
        {
            throw new RegistrationException("Basic subscription plan not found.", HttpStatusCode.InternalServerError);
        }

        var userSubscription = new UserSubscription
        {
            AssignedAt = DateTime.UtcNow,
            User = user,
            SubscriptionPlan = subscriptionPlan
        };

        user.Credential = credential;
        user.Info = userInfo;
        user.Subscription = userSubscription;

        var savedUser = await userRepository.SaveUserAsync(user);

        return mapper.Map<RegistrationResponseDto>(savedUser);
    }

    private async Task<bool> UserExists(string email)
    {
        var user = await userRepository.FindUserCredentialByEmailAsync(email);
        return user != null;
    }
}