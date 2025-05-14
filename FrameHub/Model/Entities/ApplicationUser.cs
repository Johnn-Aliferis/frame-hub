using Microsoft.AspNetCore.Identity;

namespace FrameHub.Model.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; } = true;
}