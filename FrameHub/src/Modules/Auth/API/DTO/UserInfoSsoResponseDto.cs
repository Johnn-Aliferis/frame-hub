namespace FrameHub.Modules.Auth.API.DTO;

public class UserInfoSsoResponseDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string LoginProvider { get; set; }
    public required string ProviderKey { get; set; }
}