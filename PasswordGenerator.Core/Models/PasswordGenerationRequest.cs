namespace PasswordGenerator.Core.Models;

/// <summary>
/// Represents the input parameters required for deterministic password generation using Argon2id.
/// All properties use init-only setters to ensure immutability after construction.
/// </summary>
public class PasswordGenerationRequest
{
    public required string MasterPassword { get; init; }
    public required string Seed { get; init; }
    public int Length { get; init; } = 16;
    public bool IncludeSpecialChars { get; init; } = true;
}