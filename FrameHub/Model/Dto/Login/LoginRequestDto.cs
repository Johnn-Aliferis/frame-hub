using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Login;

public class LoginRequestDto
{
    [Required]
    public required string LoginMethod { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    // public string? SsoToken { get; set; }
}