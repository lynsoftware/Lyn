using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lyn.Backend.Configuration.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lyn.Backend.Services;

public class JwtService(IOptions<JwtSettings> jwtSettings) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
  
    /// <summary>
    /// Lager en JwtToken til en bruker som logger inn.
    /// </summary>
    /// <param name="userId">BrukerId er svært ofte med i claims</param>
    /// <param name="email">Hvis vi trenger å ha epost i claims</param>
    /// <param name="roles">Hvis vi har opprettet roller</param>
    /// <returns>Ferdig token som en string</returns>
    public string GenerateJwtToken(string userId, string email, IEnumerable<string>? roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, userId),
            new (JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
      
        if (roles != null)
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
      
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddSeconds(-5),
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenValidityMinutes),
            signingCredentials: credentials
        );


        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
