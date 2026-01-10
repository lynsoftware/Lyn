using Microsoft.AspNetCore.Identity;

namespace Lyn.Backend.Models;

public class ApplicationUser : IdentityUser
{
    // IdentityUser egenskaper:
  
    // - Id (string)
    // - UserName (string)
    // - Email (string)
    // - EmailConfirmed (bool)
    // - PasswordHash (string)
    // - PhoneNumber (string)
    // - PhoneNumberConfirmed (bool)
    // - TwoFactorEnabled (bool)
    // - LockoutEnd (DateTimeOffset?)
    // - LockoutEnabled (bool)
    // - AccessFailedCount (int)
}
