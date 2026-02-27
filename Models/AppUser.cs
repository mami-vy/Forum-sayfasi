using Microsoft.AspNetCore.Identity;

namespace mym.Models;

public class AppUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? LegalTermsAcceptedAtUtc { get; set; }
    // Guest account support
    public bool IsGuest { get; set; }
    public DateTime? GuestExpiresAtUtc { get; set; }
}
