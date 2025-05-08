namespace FrameHub.Model.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; } = true;
}