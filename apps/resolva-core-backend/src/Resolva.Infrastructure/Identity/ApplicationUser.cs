using Microsoft.AspNetCore.Identity;

namespace Resolva.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
