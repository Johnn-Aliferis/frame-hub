using Microsoft.AspNetCore.Authentication;

namespace FrameHub.Model.Dto.Sso;

public class SsoChallengeResultDto
{
    public required string Provider { get; set; }
    public required AuthenticationProperties Properties { get; set; }
}