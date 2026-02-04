using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;

public interface IPasswordService
{
    /// <summary>
    /// Generates a deterministic password using Argon2id hashing algorithm.
    /// The same master password and seed will always produce the same output password.
    /// </summary>
    /// <param name="request">PasswordGenerationRequest - The user's master password used as the primary input,
    /// the seed value that makes each password unique, desired length of the generated password and
    /// whether to include special characters in the generated password</param>
    /// <returns>A deterministically generated password string</returns>
    /// <exception cref="ArgumentException">Thrown when masterPassword or seed is null, empty, or whitespace</exception>
    Task<Result<PasswordGenerationResponse>> GeneratePasswordAsync(PasswordGenerationRequest request);
}