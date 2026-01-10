using Lyn.Shared.Models;
using Lyn.Shared.Result;

namespace Lyn.Web.Services.Api;

public interface IPasswordGenerationService
{
    /// <summary>
    /// Generates a deterministic password by sending a request to the backend API.
    /// Uses Argon2id hashing algorithm on the server to derive a unique password from the master password and seed.
    /// </summary>
    /// <param name="request">The password generation parameters including master password, seed, length, and
    /// special character preference</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A Result object containing either:
    /// - Success: PasswordGenerationResponse with the generated password
    /// - Failure: Error message describing what went wrong
    /// </returns>
    Task<Result<PasswordGenerationResponse>> GeneratePasswordAsync(PasswordGenerationRequest request,
        CancellationToken cancellationToken = default);
}