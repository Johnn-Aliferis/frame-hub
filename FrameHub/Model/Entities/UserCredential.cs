namespace FrameHub.Model.Entities;

// Making use of fluent API configuration , so no need to explicitly declare length in POCO classes
#pragma warning disable CS8618 // Non-nullable field is uninitialized
public class UserCredential : BaseEntity
{
    public long UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Provider { get; set; }
    public string? ExternalId { get; set; }

    public virtual required User User { get; set; }
}
#pragma warning restore CS8618