using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Auth.API.DTO;

public class SsoRegistrationRequestDto : IRegistrationInfo
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string DisplayName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public required string LoginProvider { get; set; }
    
    [Required]
    public required string ProviderKey { get; set; }
}