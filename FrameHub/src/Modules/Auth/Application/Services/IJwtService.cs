namespace FrameHub.Modules.Auth.Application.Services;

public interface IJwtService
{
    string GenerateJwtToken(string userId, string email);
}