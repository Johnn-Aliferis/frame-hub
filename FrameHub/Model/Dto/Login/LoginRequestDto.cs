using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Login;

public class LoginRequestDto
{
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required string Password { get; set; }
}