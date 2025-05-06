using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Registration;

public class RegistrationRequestDto
{
    [Required]
    public required string RegistrationMethod { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
    [Required]
    public required string DisplayName { get; set; }
    
    public int PhoneNumber { get; set; }
}