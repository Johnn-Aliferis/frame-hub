using Microsoft.AspNetCore.Identity;

namespace FrameHub.Model.Dto.Sso;

public class UserInfoSsoResponseDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Provider { get; set; }
    
    public required ExternalLoginInfo ExternalLoginInfo { get; set; }
}