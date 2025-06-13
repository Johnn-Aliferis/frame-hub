using Microsoft.AspNetCore.Identity;

namespace FrameHub.Model.Entities;

public class Photo : BaseEntity
{
    public required string UserId { get; set; }
    public required string StorageUrl { get; set; }
    public string? CdnUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsProfilePicture { get; set; }
    public virtual required ApplicationUser User { get; set; }
}