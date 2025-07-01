using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Media;

public class PhotoRequestDto
{
    [Required]
    public string StorageKey { get; set; }
    [Required]
    public string FileName { get; set; }
    [Required]
    public bool IsProfilePicture { get; set; }
    
    public string[] Tags { get; set; } = [];
}