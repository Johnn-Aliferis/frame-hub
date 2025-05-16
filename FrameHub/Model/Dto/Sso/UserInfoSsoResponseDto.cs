namespace FrameHub.Model.Dto.Sso;

public class UserInfoSsoResponseDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Provider { get; set; }
}