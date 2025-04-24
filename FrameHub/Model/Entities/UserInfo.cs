namespace FrameHub.Model.Entities;

public class UserInfo : BaseEntity
{
    public required string DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }

    public int ProfilePictureId { get; set; }
    public int UserId { get; set; }

    public required User User { get; set; }
    public Photo? ProfilePicture { get; set; }
}