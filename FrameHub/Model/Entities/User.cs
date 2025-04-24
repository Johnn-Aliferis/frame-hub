namespace FrameHub.Model.Entities;

public class User : BaseEntity
{
    public DateTime? LastLogin { get; set; }
    
    public virtual UserCredential? Credential { get; set; }
    public virtual UserInfo? Info { get; set; }
    public virtual UserSubscription? Subscription { get; set; }
    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
}