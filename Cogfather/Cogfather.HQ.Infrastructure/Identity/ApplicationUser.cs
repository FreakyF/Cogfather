using Microsoft.AspNetCore.Identity;

namespace Cogfather.HQ.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? TotpSecret { get; set; }
    public bool IsTotpEnabled { get; set; }
}