using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lyn.Backend.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BasicAuthAttribute(string realm = "Admin") : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(
                context.HttpContext.Request.Headers.Authorization.ToString());
            
            if (authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                
                if (credentials.Length != 2)
                {
                    SetUnauthorizedResult(context);
                    return;
                }
                
                var username = credentials[0];
                var password = credentials[1];

                var configuration = context.HttpContext
                    .RequestServices.GetRequiredService<IConfiguration>();
                
                var validUsername = configuration["AdminSettings:Username"];
                var validPassword = configuration["AdminSettings:Password"];

                if (!string.IsNullOrEmpty(validUsername) && 
                    !string.IsNullOrEmpty(validPassword) &&
                    username == validUsername && 
                    password == validPassword)
                {
                    return; // Authorized
                }
            }
        }
        catch
        {
            // Fall through to unauthorized
        }

        SetUnauthorizedResult(context);
    }

    private void SetUnauthorizedResult(AuthorizationFilterContext context)
    {
        context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{realm}\"";
        context.Result = new UnauthorizedResult();
    }
}