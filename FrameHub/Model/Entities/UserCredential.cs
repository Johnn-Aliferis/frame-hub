namespace FrameHub.Model.Entities;

public class UserCredential : BaseEntity
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public required User User { get; set; }
}