using System.Net;
using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Application.Exception;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Shared.Application.Interface;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace FrameHub.Tests.Modules.Auth.Application.Services;

public class RegistrationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepoMock = new();
    private readonly Mock<ILogger<RegistrationService>> _loggerMock = new();
    private readonly RegistrationService _service;

    public RegistrationServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _service = new RegistrationService(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _userManagerMock.Object,
            _subscriptionPlanRepoMock.Object);
    }

    [Fact]
    public async Task RegisterDefaultAsync_Success()
    {
        var request = new DefaultRegistrationRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User",
            PhoneNumber = "1234567890"
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _subscriptionPlanRepoMock.Setup(s => s.FindSubscriptionPlanByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new SubscriptionPlan
                { Id = 1, Name = "Basic", Code = "BASIC", ProductId = "test_product_id", PriceId = "test_price_id" });

        _userRepositoryMock.Setup(r => r.SaveUserInfoAsync(It.IsAny<UserInfo>()))
            .ReturnsAsync(new UserInfo
            {
                Id = 1,
                DisplayName =  "Test User",
                PhoneNumber = "1234567890",
                UserId = "user_id",
                User = new ApplicationUser()
            });

        _userRepositoryMock.Setup(r => r.SaveUserSubscriptionAsync(It.IsAny<UserSubscription>()))
            .ReturnsAsync(new UserSubscription { Id = 1, UserId = "user_id", });

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var result = await _service.RegisterDefaultAsync(request);

        // Assertions
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
    }
    
    
    [Fact]
    public async Task RegisterDefaultAsync_UserExists()
    {
        var request = new DefaultRegistrationRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User",
            PhoneNumber = "1234567890"
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());
        
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var exception = await Assert.ThrowsAsync<RegistrationException>(async () => 
                await _service.RegisterDefaultAsync(request)
        );

        // Assertions
        Assert.Equal($"Email {request.Email} already exists.", exception.Message);
        Assert.Equal(HttpStatusCode.Conflict, exception.Status);
    }
    
    
    [Fact]
    public async Task RegisterDefaultAsync_Failure_User_Could_Not_Be_Created()
    {
        var request = new DefaultRegistrationRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User",
            PhoneNumber = "1234567890"
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed());

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var exception = await Assert.ThrowsAsync<RegistrationException>(async () => 
            await _service.RegisterDefaultAsync(request)
        );

        // Assertions
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }
    
    
    [Fact]
    public async Task RegisterDefaultAsync_Failure_SubscriptionPlan_Could_Not_Be_Found()
    {
        var request = new DefaultRegistrationRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User",
            PhoneNumber = "1234567890"
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _subscriptionPlanRepoMock.Setup(s => s.FindSubscriptionPlanByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((SubscriptionPlan?)null);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var exception = await Assert.ThrowsAsync<RegistrationException>(async () => 
            await _service.RegisterDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("Basic subscription plan not found.", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }
    
    
    [Fact]
    public async Task RegisterSsoAsync_Success()
    {
        var request = new SsoRegistrationRequestDto
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PhoneNumber = "1234567890",
            LoginProvider =  "Google",
            ProviderKey = "test_provider_key",
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManagerMock.Setup(m => m.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);

        _subscriptionPlanRepoMock.Setup(s => s.FindSubscriptionPlanByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new SubscriptionPlan
                { Id = 1, Name = "Basic", Code = "BASIC", ProductId = "test_product_id", PriceId = "test_price_id" });

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var result = await _service.RegisterSsoAsync(request);

        // Assertions
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Email, result.UserName);
    }
    
    
    [Fact]
    public async Task RegisterSsoAsync_Failure_Email_exists()
    {
        var request = new SsoRegistrationRequestDto
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PhoneNumber = "1234567890",
            LoginProvider =  "Google",
            ProviderKey = "test_provider_key",
        };

        // Setup mocks
        _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        var exception = await Assert.ThrowsAsync<RegistrationException>(async () => 
            await _service.RegisterSsoAsync(request)
        );

        // Assertions
        Assert.Equal($"Email {request.Email} already exists.", exception.Message);
        Assert.Equal(HttpStatusCode.Conflict, exception.Status);
    }
    
}