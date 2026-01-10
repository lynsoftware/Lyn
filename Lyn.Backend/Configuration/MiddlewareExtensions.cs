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
        app.UseHttpsRedirection();
        
        app.UseCors("AllowBlazorApp"); 
      
        // Endpoints
        app.MapControllers();


        return app;
    }
}
