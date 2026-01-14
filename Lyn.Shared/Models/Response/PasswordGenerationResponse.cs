namespace Lyn.Shared.Models.Response;

/// <summary>
/// Represents the result of a password generation operation.
/// Encapsulates the generated password value for type-safe handling and future extensibility.
/// </summary>
public class PasswordGenerationResponse
{
    public string Value { get; set; } = string.Empty;
}