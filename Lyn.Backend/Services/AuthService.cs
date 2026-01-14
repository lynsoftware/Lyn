
using Lyn.Backend.Models;
using Lyn.Shared.Models;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Identity;

namespace Lyn.Backend.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    ILogger<AuthService> logger,
    IJwtService jwtService) : IAuthService
{
    public async Task<Result<string>> LoginAsync(LoginRequest request)
    {
        logger.LogInformation("LoginAsync. Payload: {@Payload}", new { request.Email });

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            logger.LogWarning("Login failed. User not found for {Email}", request.Email);
        }

        // Sjekk om kontoen er låst
        if (user != null && await userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
            logger.LogWarning("Login failed. Account locked for {Email} until {LockoutEnd}",
                request.Email, lockoutEnd);
            return Result<string>.Failure(
                "Your account has been locked due to multiple failed login attempts. " +
                "Please try again later.");
        }

        // Bruk dummy user for timing attack protection
        var targetUser = user ?? DummyUser;

        var isPasswordValid = await userManager.CheckPasswordAsync(targetUser, request.Password);

        if (user == null || !isPasswordValid)
        {
            if (user != null)
                await userManager.AccessFailedAsync(user);

            logger.LogWarning("Login failed. Invalid credentials for {Email}", request.Email);
            return Result<string>.Failure("Wrong email or password");
        }

        // Reset failed access count ved vellykket login
        await userManager.ResetAccessFailedCountAsync(user);

        // Hent brukerens roller
        var roles = await userManager.GetRolesAsync(user);

        // Generer JWT token
        var token = jwtService.GenerateJwtToken(user.Id, user.Email!, roles);

        logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Result<string>.Success(token);
    }
    
    private static readonly ApplicationUser DummyUser = new()
    {
        Id = "00000000-0000-0000-0000-000000000000",
        UserName = "dummy@example.com",
        NormalizedUserName = "DUMMY@EXAMPLE.COM",
        Email = "dummy@example.com",
        NormalizedEmail = "DUMMY@EXAMPLE.COM",
        EmailConfirmed = false,
        PasswordHash = "AQAAAAIAAYagAAAAEFxKhF7EhQlH5n9sVHvKmQx3Z8tYvN2p" +
                       "WqR4sT6uV7wX8yZ9aA1bB2cC3dD4eE5fF6gG7hH8iI9jJ0kK1l" +
                       "L2mM3nN4oO5pP6qQ7rR8sS9tT0uU1vV2wW3xX4yY5zZ6aA7bB8c" +
                       "C9dD0eE1fF2gG3hH4iI5jJ6kK7lL8mM9nN0oO1pP2qQ3rR4sS5tT" +
                       "6uU7vV8wW9xX0yY1zZ2=="
    };
    
}