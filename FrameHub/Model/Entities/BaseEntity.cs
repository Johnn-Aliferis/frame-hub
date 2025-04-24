namespace FrameHub.Model.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public Guid Guid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; }
}