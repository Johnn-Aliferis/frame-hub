namespace FrameHub.Modules.Auth.API.DTO;

public class RegistrationResponseDto
{
    public required string UserId { get; set; } 
    public required string Email { get; set; } 
    public required string UserName { get; set; }
}