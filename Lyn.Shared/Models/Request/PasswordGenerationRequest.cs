using System.ComponentModel.DataAnnotations;

namespace Lyn.Shared.Models.Request;

/// <summary>
/// Represents the input parameters required for deterministic password generation using Argon2id.
/// All properties use init-only setters to ensure immutability after construction.
/// </summary>
public class PasswordGenerationRequest
{
    [Required(ErrorMessage = "Master Password is required")]
    public string MasterPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Seed is required")]
    public string Seed { get; set; } = string.Empty;
    public int Length { get; set; } = 16;
    public bool IncludeSpecialChars { get; set; } = true;
}