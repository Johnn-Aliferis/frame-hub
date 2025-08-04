using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Media.API.DTO;

public class PhotoRequestDto
{
    [Required]
    public string StorageKey { get; set; }
    [Required]
    public string FileName { get; set; }
    [Required]
    public bool IsProfilePicture { get; set; }
    
    public string? Tags { get; set; }
}