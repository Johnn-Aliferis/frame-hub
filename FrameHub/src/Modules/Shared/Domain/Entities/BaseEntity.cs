namespace FrameHub.Modules.Shared.Domain.Entity;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; } = true;
    
    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }
}