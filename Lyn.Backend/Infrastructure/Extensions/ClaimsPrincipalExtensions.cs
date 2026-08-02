using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lyn.Backend.Infrastructure.Extensions;

/// <summary>
/// Extension for å hente ut claims fra JwtToken
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Henter ut UserId fra token (NameIdentifier). Brukes i kontrollerne da den kaster feil
    /// </summary>
    /// <param name="user">Brukeren som har sendt en forespørsel</param>
    /// <returns>UserId som string</returns>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("UserId not found in token");

        return userId;
    }
}
