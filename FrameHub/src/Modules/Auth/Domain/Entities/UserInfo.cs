using FrameHub.Modules.Media.Domain.Entity;
using FrameHub.Modules.Shared.Domain.Entity;

namespace FrameHub.Modules.Auth.Domain.Entities;

public class UserInfo : BaseEntity
{
    public required string DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }

    public long? ProfilePictureId { get; set; }
    public required string UserId { get; set; }

    public virtual required ApplicationUser User { get; set; }
    public virtual Photo? ProfilePicture { get; set; }
}