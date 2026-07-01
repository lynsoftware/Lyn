using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lyn.Backend.Platform.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lyn.Backend.Platform.Auth.Services;

/// <inheritdoc />
public class JwtService(IOptions<JwtSettings> jwtSettings) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    
    public string GenerateJwtToken(string userId, string email, string language, IEnumerable<string>? roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, userId),
            new (JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("lang", language) 
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
