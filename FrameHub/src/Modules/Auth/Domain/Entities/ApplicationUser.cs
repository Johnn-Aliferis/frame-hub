using Microsoft.AspNetCore.Identity;

namespace FrameHub.Modules.Auth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Status { get; set; } = true;
    
    public ApplicationUser()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }
}