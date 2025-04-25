namespace FrameHub.Model.Entities;

public class Photo : BaseEntity
{
    public required long UserId { get; set; }
    public required string StorageUrl { get; set; }
    public required string CdnUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsProfilePicture { get; set; }

    public virtual required User User { get; set; }
}