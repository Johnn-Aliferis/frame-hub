using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Auth.API.DTO;

public class LoginRequestDto
{
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required string Password { get; set; }
}