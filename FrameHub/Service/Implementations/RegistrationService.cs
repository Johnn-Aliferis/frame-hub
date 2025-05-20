using System.Net;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Interface;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FrameHub.Service.Implementations;

public class RegistrationService(
    ILogger<RegistrationService> logger,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    UserManager<ApplicationUser> userManager,
    ISubscriptionPlanRepository subscriptionPlanRepository) : IRegistrationService
{
    public async Task<RegistrationResponseDto> RegisterDefaultAsync(DefaultRegistrationRequestDto defaultRegistrationRequestDto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var response = await HandleDefaultRegistration(defaultRegistrationRequestDto);
            await unitOfWork.CommitAsync();
            return response;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "An error occurred during user registration.");
            throw ;
        }
    }

    public async Task<ApplicationUser> RegisterSsoAsync(SsoRegistrationRequestDto ssoRegistrationRequestDto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var response = await HandleSsoRegistration(ssoRegistrationRequestDto);
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

    private async Task<RegistrationResponseDto> HandleDefaultRegistration(DefaultRegistrationRequestDto defaultRegistrationRequestDto)
    {
        if (await UserExists(defaultRegistrationRequestDto.Email))
        {
            throw new RegistrationException($"Email {defaultRegistrationRequestDto.Email} already exists.",
                HttpStatusCode.Conflict);
        }

        var applicationUser = new ApplicationUser
        {
            UserName = defaultRegistrationRequestDto.Email,
            Email = defaultRegistrationRequestDto.Email,
        };

        var response = await CreateUserAsync(applicationUser, defaultRegistrationRequestDto.Password);

        return await HandleUserInfoSave(response, defaultRegistrationRequestDto);
    }

    private async Task<ApplicationUser> HandleSsoRegistration(SsoRegistrationRequestDto ssoRegistrationRequestDto)
    {
        // for same email, different providers conflict.
        if (await UserExists(ssoRegistrationRequestDto.Email))
        {
            throw new RegistrationException($"Email {ssoRegistrationRequestDto.Email} already exists.",
                HttpStatusCode.Conflict);
        }
        
        var applicationUser = new ApplicationUser
        {
            UserName = ssoRegistrationRequestDto.Email,
            Email = ssoRegistrationRequestDto.Email,
        };

        var createResult = await userManager.CreateAsync(applicationUser);
        if (!createResult.Succeeded)
        {
            throw new RegistrationException("User creation failed", HttpStatusCode.BadRequest);
        }

        await HandleUserInfoSave(applicationUser, ssoRegistrationRequestDto);

        return applicationUser;
    }


    private async Task<RegistrationResponseDto> HandleUserInfoSave(ApplicationUser response,
        IRegistrationInfo registrationInfoRequestDto)
    {
        var userInfo = new UserInfo
        {
            DisplayName = registrationInfoRequestDto.DisplayName,
            PhoneNumber = registrationInfoRequestDto.PhoneNumber,
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
            User = response,
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


    private async Task<ApplicationUser> CreateUserAsync(ApplicationUser applicationUser, string password)
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