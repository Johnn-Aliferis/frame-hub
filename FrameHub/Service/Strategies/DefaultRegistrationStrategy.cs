using System.Net;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Service.Strategies;

public class DefaultRegistrationStrategy(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ILogger<DefaultRegistrationStrategy> logger,
    ISubscriptionPlanRepository subscriptionPlanRepository, 
    UserManager<ApplicationUser> userManager) : IRegistrationStrategy
{
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

        var applicationUser = new ApplicationUser
        {
            UserName = registrationRequestDto.Email,
            Email = registrationRequestDto.Email,
        };
        
        var response = await CreateUserAsync(applicationUser , registrationRequestDto.Password);
        
        
        var userInfo = new UserInfo
        {
            DisplayName = registrationRequestDto.DisplayName,
            PhoneNumber = registrationRequestDto.PhoneNumber,
            User = response,
            UserId = response.Id,
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
            User = applicationUser,
            UserId = response.Id,
            SubscriptionPlan = subscriptionPlan
        };

        await userRepository.SaveUserInfoAsync(userInfo);
        await userRepository.SaveUserSubscriptionAsync(userSubscription);

        var registrationResponse = new RegistrationResponseDto
        {
            UserId = response.Id,
            Email = response.Email!,
            UserName = response.Email!
        };

        return registrationResponse;
    }

    private async Task<bool> UserExists(string email)
    {
        var user = await userRepository.FindUserByEmailAsync(email);
        return user != null;
    }


    private async Task<ApplicationUser> CreateUserAsync(ApplicationUser applicationUser , string password)
    {
        var result = await userManager.CreateAsync(applicationUser, password);
        if (!result.Succeeded)
        {
            throw new RegistrationException(string.Join(", ", result.Errors.Select(e => e.Description)),
                HttpStatusCode.InternalServerError);
        }
        return applicationUser;
    }
}