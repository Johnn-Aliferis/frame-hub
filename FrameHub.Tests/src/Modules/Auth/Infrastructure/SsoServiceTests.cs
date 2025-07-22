using System.Net;
using System.Security.Claims;
using FrameHub.Modules.Auth.API.DTO;
using FrameHub.Modules.Auth.Application.Exception;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Auth.Domain.Enum;
using FrameHub.Modules.Auth.Infrastructure.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace FrameHub.Tests.Modules.Auth.Infrastructure;

public class SsoServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRegistrationService> _registrationServiceMock = new();
    private readonly Mock<ILoginService> _loginServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly SsoService _service;

    public SsoServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        
        _service = new SsoService(
            _userRepositoryMock.Object,
            _registrationServiceMock.Object,
            _userManagerMock.Object,
            _loginServiceMock.Object);
    }

    [Fact]
    public void HandleSsoStart_Failure_Provider_Is_Empty()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        var exception = Assert.Throws<SsoException>(() => _service.HandleSsoStart(null!, urlHelperMock.Object));
        Assert.Equal("Provider is required", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    [Fact]
    public void HandleSsoStart_Failure_Provider_Is_Unsupported()
    {
        const string provider = "Test";
        var urlHelperMock = new Mock<IUrlHelper>();
        var exception = Assert.Throws<SsoException>(() => _service.HandleSsoStart(provider, urlHelperMock.Object));
        Assert.Equal("Unsupported provider", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    
    [Fact]
    public void HandleSsoStart_Failure_Redirect_Url_Is_Null()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        string expectedUrl = null!;
        
        urlHelperMock.Setup(x => x.Action(It.Is<UrlActionContext>(context =>
                context.Action == "Register" &&
                context.Controller == "Sso" &&
                context.Values != null &&
                context.Values.GetType().GetProperty("provider") != null &&
                context.Protocol == "https")))
            .Returns(expectedUrl);

        var exception = Assert.Throws<SsoException>(() =>
            _service.HandleSsoStart(SsoProviderEnum.Google.ToString(), urlHelperMock.Object));

        Assert.Equal("Invalid or unsupported provider", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    [Fact]
    public void HandleSsoStart_Success()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        const string expectedUrl = "https://test.com/sso/register";

        urlHelperMock.Setup(x => x.Action(It.Is<UrlActionContext>(context =>
                context.Action == "Register" &&
                context.Controller == "Sso" &&
                context.Values != null &&
                context.Values.GetType().GetProperty("provider") != null &&
                context.Protocol == "https")))
            .Returns(expectedUrl);

        var result = _service.HandleSsoStart(SsoProviderEnum.Google.ToString(), urlHelperMock.Object);

        Assert.NotNull(result);
        Assert.Equal(SsoProviderEnum.Google.ToString(), result.Provider);
        Assert.NotNull(result.Properties);
        Assert.Equal(expectedUrl, result.Properties.RedirectUri);
    }
    
    [Fact]
    public async Task HandleCallBackAsync_Success()
    {
        var user = new ApplicationUser { Email = "user@example.com" };
        var loginResponse = new LoginResponseDto();

        var result = CreateAuthenticateResult("user@example.com", "google", "test_provider");

        _userManagerMock
            .Setup(x => x.FindByLoginAsync("google", "test_provider"))
            .ReturnsAsync(user);

        _loginServiceMock
            .Setup(x => x.SsoLogin(user))
            .Returns(loginResponse);
        
        var response = await _service.HandleCallbackAsync(result);
        Assert.Equal(loginResponse, response);
    }
    
    [Fact]
    public async Task HandleCallBackAsync_Failure_Exists_Under_Different_Provider()
    {
        var result = CreateAuthenticateResult("user@example.com", "google", "test_provider");

        _userManagerMock
            .Setup(x => x.FindByLoginAsync("google", "test_provider"))
            .ReturnsAsync((ApplicationUser?)null);

        _userRepositoryMock
            .Setup(x => x.FindUserByEmailAsync("user@example.com"))
            .ReturnsAsync(new ApplicationUser());
        
        var exception = await Assert.ThrowsAsync<SsoException>(() => _service.HandleCallbackAsync(result));
        
        Assert.Equal(HttpStatusCode.Conflict, exception.Status);
        Assert.Equal("This email is already registered with a different provider.", exception.Message);
    }
    
    [Fact]
    public async Task HandleCallbackAsync_Failure()
    {
        var exception = await Assert.ThrowsAsync<SsoException>(() => _service.HandleCallbackAsync(null!));
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    private static AuthenticateResult CreateAuthenticateResult(string email, string provider, string providerKey)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.NameIdentifier, providerKey),
            new(ClaimTypes.GivenName, "John"),
            new(ClaimTypes.Surname, "Peterson")
        };

        var identity = new ClaimsIdentity(claims, "sso");
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties();
        props.Items[".AuthScheme"] = provider;

        return AuthenticateResult.Success(new AuthenticationTicket(principal, props, provider));
    }

}