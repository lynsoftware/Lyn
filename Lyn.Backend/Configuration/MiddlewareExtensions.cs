using Lyn.Backend.Data;
using Serilog;

namespace Lyn.Backend.Configuration;

public static class MiddlewareExtensions
{
    

    
    /// <summary>
    /// Konfigurerer hele middleware-pipelinen for applikasjonen.
    /// Middleware kjører i rekkefølgen de legges til, og denne rekkefølgen er kritisk for sikkerhet og funksjonalitet.
    /// </summary>
    /// <param name="app">WebApplication-instansen som skal konfigureres</param>
    /// <returns>Den konfigurerte WebApplication-instansen for method chaining</returns>
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        
        // Global CORS middleware - MÅ KOMME FØRST for å håndtere alle responses inkludert errors
        app.Use(async (context, next) =>
        {
            var origin = context.Request.Headers.Origin.FirstOrDefault();
            var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    
            // Alltid legg til CORS headers hvis origin matcher
            if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
            {
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    {
                        context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                        context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
                        context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
                        context.Response.Headers.Append("Access-Control-Max-Age", "600");
                    }
                    return Task.CompletedTask;
                });
            }
    
            // Handle preflight
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return;
            }
    
            await next();
        });
        
        // Exception Handling
        app.UseExceptionHandler();
      
        // Logging
        app.UseSerilogRequestLogging();


        // Development Tools
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        // Security & Routing
        app.UseCors("AllowBlazorApp"); 
        // app.UseHttpsRedirection();
        
        
        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        

        
        // Endpoints & healthcheck
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        })).AllowAnonymous();
        app.MapControllers();


        return app;
    }
    
    /// <summary>
    /// Seeder databasen med initielle data (roller og admin-bruker)
    /// Kjøres én gang ved oppstart før middleware-pipelinen konfigureres
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
