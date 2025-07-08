using FrameHub.Modules.Auth.Infrastructure.Configuration;
using FrameHub.Modules.Auth.Infrastructure.Service;
using FrameHub.Modules.Shared.Application.Exception;
using Microsoft.Extensions.Options;

namespace FrameHub.Tests.Modules.Auth.Infrastructure;

public class JwtServiceTests
{
    [Fact]
    public void GenerateJwtToken_ShouldReturnToken_WhenValidInput()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "testsecretkey12345678900987654321");
        var options = Options.Create(new JwtSettingsOptions
        {
            Issuer = "FrameHub",
            Audience = "FrameHubUsers"
        });

        var service = new JwtService(options);
        var token = service.GenerateJwtToken("user-id", "user@example.com");
        
        Assert.False(string.IsNullOrWhiteSpace(token));
    }
    
    [Fact]
    public void GenerateJwtToken_ShouldThrow_WhenSecretIsMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        var options = Options.Create(new JwtSettingsOptions
        {
            Issuer = "FrameHub",
            Audience = "FrameHubUsers"
        });

        var service = new JwtService(options);
        
        var exception = Assert.Throws<GeneralException>(() =>
            service.GenerateJwtToken("user-id", "user@example.com"));

        Assert.Equal("JWT_SECRET environment variable is missing.", exception.Message);
    }
}