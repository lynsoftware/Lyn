using System.Text;
using Konscious.Security.Cryptography;
using PasswordGenerator.Configuration;
using PasswordGenerator.Models;
using PasswordGenerator.Resources.Strings;


namespace PasswordGenerator.Services;

public class PasswordService : IPasswordService
{
    // Check interface for summary
    public async Task<GeneratedPassword> GeneratePasswordAsync(PasswordGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MasterPassword))
            throw new ArgumentException(AppResources.MasterPasswordEmpty, nameof(request.MasterPassword));
        
        if (string.IsNullOrWhiteSpace(request.Seed))
            throw new ArgumentException(AppResources.SeedRequired, nameof(request.Seed));
        
        if (request.Length < AppConstants.PasswordMinLength || request.Length > AppConstants.PasswordMaxLength)
            throw new ArgumentException(
                string.Format(AppResources.LengthValidationError, 
                    AppConstants.PasswordMinLength, 
                    AppConstants.PasswordMaxLength),
                nameof(request.Length));
        
        
        return await Task.Run(() =>
        {
        
            // Convert the master password and seed strings to byte arrays for cryptographic processing
            byte[] passwordBytes = Encoding.UTF8.GetBytes(request.MasterPassword);
            byte[] saltBytes = Encoding.UTF8.GetBytes(request.Seed);
            
            // Configure and initialize Argon2id with security parameters
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = saltBytes,              // The seed acts as the salt, ensuring unique outputs for different services
                DegreeOfParallelism = 4,       // Use 4 parallel threads for computation
                MemorySize = 131072,            // Allocate 128 MB of memory (makes GPU/ASIC attacks more difficult)
                Iterations = 4                  // Number of iterations (more iterations = slower but more secure)
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

            return new GeneratedPassword
            {
                Value = password.ToString()
            };
        });
    }
}