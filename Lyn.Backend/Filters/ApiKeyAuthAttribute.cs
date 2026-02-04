using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lyn.Backend.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthAttribute>>();

        var validApiKey = configuration["ReleaseApiKey"];

        if (string.IsNullOrEmpty(validApiKey))
        {
            logger.LogError("ReleaseApiKey is not configured");
            context.Result = new StatusCodeResult(500);
            return;
        }
        
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey) ||
            apiKey != validApiKey)
        {
            logger.LogWarning("Unauthorized release upload attempt");
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid API key" });
            return;
        }
        
        await next();
    }
}