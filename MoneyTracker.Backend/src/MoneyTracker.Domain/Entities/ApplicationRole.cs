using Microsoft.AspNetCore.Identity;
using MoneyTracker.Domain.Common;

namespace MoneyTracker.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ApplicationRole() : base()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public ApplicationRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}