using System.ComponentModel.DataAnnotations;

namespace FrameHub.Modules.Media.API.DTO;

public class PresignedUrlRequestDto
{
    [Required]
    public string FileName { get; set; }
}