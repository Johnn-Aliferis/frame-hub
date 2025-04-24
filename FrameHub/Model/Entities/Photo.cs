namespace FrameHub.Model.Entities;

public class Photo : BaseEntity
{
    public int UserId { get; set; }
    public required string StorageUrl { get; set; }
    public required string CdnUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsProfilePicture { get; set; }

    public required User User { get; set; }
}