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
