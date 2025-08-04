namespace FrameHub.Modules.Media.API.DTO;

public class PhotoResponseDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; }
    public Guid Guid { get; set; }
    public string StorageKey { get; set; }
    public string Provider { get; set; }
    public string FileName { get; set; }
    public string Tags { get; set; }
    public bool IsProfilePicture { get; set; }   
}