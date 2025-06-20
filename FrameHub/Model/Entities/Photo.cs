using Microsoft.AspNetCore.Identity;

namespace FrameHub.Model.Entities;

public class Photo : BaseEntity
{
    public required string UserId { get; set; }
    public string? StorageUrl { get; set; }
    public string? CdnUrl { get; set; }
    public string? Tags { get; set; }
    public required string UploadState { get; set; }
    public bool IsProfilePicture { get; set; }
    public virtual ApplicationUser? User { get; set; }
}