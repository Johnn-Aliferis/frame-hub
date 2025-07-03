using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Shared.Domain.Entity;

namespace FrameHub.Modules.Media.Domain.Entity;

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