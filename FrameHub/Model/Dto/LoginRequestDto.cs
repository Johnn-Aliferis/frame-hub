namespace FrameHub.Model.Dto;

public class LoginRequestDto
{
    public required string LoginMethod { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? SsoToken { get; set; }
}