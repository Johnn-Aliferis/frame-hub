namespace FrameHub.Model.Entities;

public class Photo : BaseEntity
{
    public required string UserId { get; set; }
    public required string StorageKey { get; set; }
    public required string Provider { get; set; }
    public required string FileName { get; set; }
    public string? Tags { get; set; }
    public bool IsProfilePicture { get; set; }
    public virtual ApplicationUser? User { get; set; }
}