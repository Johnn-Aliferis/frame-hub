using System.ComponentModel.DataAnnotations;

namespace FrameHub.Model.Dto.Media;

public class PresignedUrlRequestDto
{
    [Required]
    public string FileName { get; set; }
}