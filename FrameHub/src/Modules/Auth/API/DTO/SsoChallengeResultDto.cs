using Microsoft.AspNetCore.Authentication;

namespace FrameHub.Modules.Auth.API.DTO;

public class SsoChallengeResultDto
{
    public required string Provider { get; set; }
    public required AuthenticationProperties Properties { get; set; }
}