using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Auth.API.DTO;

public class DefaultRegistrationRequestDto : IRegistrationInfo
{
    // [Required]
    // [AllowedValues("default", "sso")]
    // public required string RegistrationMethod { get; set; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email can't be longer than 100 characters.")]
    public required string Email { get; set; }
    
    [Required]
    [RegularExpression(@"^(?=.*\d)(?=.*[\W_]).{8,25}$",
        ErrorMessage = "Password must be between 8 and 25 characters, contain at least 1 Number and 1 Symbol")]
    public required string Password { get; set; }
    
    [Required]
    [StringLength(50, ErrorMessage = "Display Name can't be longer than 50 characters.")]
    public required string DisplayName { get; set; }
    
    [Phone]
    [StringLength(20, ErrorMessage = "Phone number can't be longer than 20 digits.")]
    public string? PhoneNumber { get; set; }
}