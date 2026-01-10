using System.Text;
using Konscious.Security.Cryptography;
using Lyn.Backend.Repository;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public class PasswordService(
    ILogger<PasswordService> logger,
    IServiceScopeFactory serviceScopeFactory) : IPasswordService
{
    // Check interface for summary
    public async Task<Result<PasswordGenerationResponse>> GeneratePasswordAsync(PasswordGenerationRequest request)
    {
        if (request.Length < AppConstants.PasswordMinLength || request.Length > AppConstants.PasswordMaxLength)
            throw new ArgumentException(
                $"Length must be between {AppConstants.PasswordMinLength} and {AppConstants.PasswordMaxLength}",
                nameof(request.Length));

        var response = await Task.Run(() =>
        {
            // Convert the master password and seed strings to byte arrays for cryptographic processing
            byte[] passwordBytes = Encoding.UTF8.GetBytes(request.MasterPassword);
            byte[] saltBytes = Encoding.UTF8.GetBytes(request.Seed);

            // Configure and initialize Argon2id with security parameters
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = saltBytes, // The seed acts as the salt, ensuring unique outputs for different services
                DegreeOfParallelism = 4, // Use 4 parallel threads for computation
                MemorySize = 131072, // Allocate 128 MB of memory (makes GPU/ASIC attacks more difficult)
                Iterations = 4 // Number of iterations (more iterations = slower but more secure)
            };

            // Execute the Argon2 algorithm and generate a hash of the specified length
            // Math.Max ensures we get at least 32 bytes or more if needed
            byte[] hash = argon2.GetBytes(Math.Max(32, request.Length));

            // Define the character set for the generated password
            // Includes lowercase, uppercase, and digits by default
            string chars = AppConstants.AlphanumericChars;
            if (request.IncludeSpecialChars)
                chars += AppConstants.SpecialChars;

            StringBuilder password = new StringBuilder();

            // Convert each hash byte to a password character
            // The modulo operation maps each byte value (0-255) to a valid index in the chars string
            // This ensures deterministic mapping: same hash byte always produces the same character
            for (int i = 0; i < request.Length; i++)
            {
                password.Append(chars[hash[i] % chars.Length]);
            }
            

            return new PasswordGenerationResponse
            {
                Value = password.ToString()
            };
        });
        
        // Updates the statistictable in the background
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var statisticsRepository = scope.ServiceProvider.GetRequiredService<IStatisticsRepository>();
                await statisticsRepository.IncrementPasswordGeneratedAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to increment password generation statistics");
            }
        });
            
        return Result<PasswordGenerationResponse>.Success(response);
    }
}