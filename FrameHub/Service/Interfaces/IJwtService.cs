using FrameHub.Options;

namespace FrameHub.Service.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(string userId, string email);
}