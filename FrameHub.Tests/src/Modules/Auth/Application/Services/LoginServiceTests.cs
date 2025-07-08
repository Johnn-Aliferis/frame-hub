using System.Net;
using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Application.Exception;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Auth.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FrameHub.Tests.Modules.Auth.Application.Services;

public class LoginServiceTests
{
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly LoginService _service;
    
    public LoginServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null!, null!, null!, null!);
        
        _service = new LoginService(
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _userRepositoryMock.Object);
    }
    
    [Fact]
    public async Task LoginDefaultAsync_Success()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userRepositoryMock.Setup(x => x.FindUserByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _jwtServiceMock.Setup(x => x.GenerateJwtToken(user.Id, user.Email!))
            .Returns("mock-token");

        var result = await _service.LoginDefaultAsync(request);

        Assert.NotNull(result);
        Assert.Equal("mock-token", result.AccessToken);
    }
    
    [Fact]
    public async Task LoginDefaultAsync_Failure_Wrong_Credentials()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.Failed);
        
        var exception = await Assert.ThrowsAsync<LoginException>(async () => 
            await _service.LoginDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("Invalid email/password. Please try again", exception.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.Status);
    }
    
    [Fact]
    public async Task LoginDefaultAsync_Failure_Locked_out()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.LockedOut);
        
        var exception = await Assert.ThrowsAsync<LoginException>(async () => 
            await _service.LoginDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("Account is locked.", exception.Message);
        Assert.Equal(HttpStatusCode.Forbidden, exception.Status);
    }
    
    [Fact]
    public async Task LoginDefaultAsync_Failure_Not_Allowed()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.NotAllowed);
        
        var exception = await Assert.ThrowsAsync<LoginException>(async () => 
            await _service.LoginDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("Login not allowed. Email might not be confirmed.", exception.Message);
        Assert.Equal(HttpStatusCode.Forbidden, exception.Status);
    }
    
    [Fact]
    public async Task LoginDefaultAsync_Failure_Two_Factor()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        
        var exception = await Assert.ThrowsAsync<LoginException>(async () => 
            await _service.LoginDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("Two-factor authentication required.", exception.Message);
        Assert.Equal(HttpStatusCode.Forbidden, exception.Status);
    }
    
    
    [Fact]
    public async Task LoginDefaultAsync_Failure_User_Not_Found()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = request.Email
        };

        _signInManagerMock.Setup(x =>
                x.PasswordSignInAsync(request.Email, request.Password, false, false))
            .ReturnsAsync(SignInResult.Success);
        
        _userRepositoryMock.Setup(x => x.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        var exception = await Assert.ThrowsAsync<LoginException>(async () => 
            await _service.LoginDefaultAsync(request)
        );

        // Assertions
        Assert.Equal("User not found", exception.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.Status);
    }
    
    
    [Fact]
    public void LoginSsoAsync_Success()
    {
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "user_email"
        };
        
        _jwtServiceMock.Setup(x => x.GenerateJwtToken(user.Id, user.Email!))
            .Returns("mock-token");

        var result = _service.SsoLogin(user);

        Assert.NotNull(result);
        Assert.Equal("mock-token", result.AccessToken);
    }
}