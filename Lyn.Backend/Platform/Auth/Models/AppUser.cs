using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Lyn.Backend.Platform.Auth.Models;

public class AppUser : IdentityUser
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
    
    /// <summary>
    /// Brukerens foretrukne kultur (BCP-47, f.eks. "en" eller "nb").
    /// Stemples inn som "lang"-claim i JWT og styrer lokaliserte feilmeldinger/e-poster.
    /// </summary>
    [StringLength(10, MinimumLength = 5 )]
    public string PreferredCulture { get; set; } = "en";
    
    // ======================== Metadata  ========================
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    // ======================== Metoder ========================
    public bool IsVerified => EmailConfirmed && PhoneNumberConfirmed;
}
