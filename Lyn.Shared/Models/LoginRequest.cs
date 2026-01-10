using System.ComponentModel.DataAnnotations;

namespace Lyn.Shared.Models;

/// <summary>
/// Request with Email and Password
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(200, MinimumLength = 8, ErrorMessage = "Password must be between 8-200 characters")]
    public string Password { get; set; } = string.Empty;
}