using Lyn.Backend.Middleware;
using Lyn.Backend.Services;
using Serilog;

namespace Lyn.Backend.Configuration;

public static class ServiceExtensions
{
   /// <summary>
   /// Konfigurerer alle tjenester som applikasjonen trenger
   /// Slår sammen metodene med builderen i riktig rekkefølge
   /// </summary>
   /// <param name="builder">WebApplicationBuilder-instansen som skal konfigureres</param>
   /// <returns>Den konfigurerte WebApplicationBuilder-instansen for method chaining</returns>
   public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
  {
      // Logging
      builder.AddLogging();
      
      // CORS / RateLimiting
      builder.Services.AddCorsPolicy(builder.Configuration);
      
      // Application Services
      builder.Services.AddApplicationServices();
     
      // Infrastruktur (Database, Repositories, Services etc.)
      builder.Services.AddInfrastructure(builder.Configuration);
    
      return builder;
  }


   

  /// <summary>
  /// Konfigurerer Serilog som logging-provider for applikasjonen
  /// Logger skrives til både console og til roterende daglige loggfiler i logs/-mappen
  /// Loggfilene navngis automatisk med dato (f.eks. app-20250109.log)
  /// </summary>
  /// <param name="builder">WebApplicationBuilder-instansen som skal konfigureres</param>
  /// <returns>Den konfigurerte WebApplicationBuilder-instansen for method chaining</returns>
  private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
  {
      Log.Logger = new LoggerConfiguration()
          .WriteTo.Console()
          .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
          .CreateLogger();


      
      builder.Host.UseSerilog();
    
      return builder;
  }
  
  /// <summary>
  /// Konfigurerer CORS-policy for å tillate requests fra spesifikke origins
  /// Origins hentes fra appsettings.json og kan variere per miljø (dev/prod)
  /// </summary>
  /// <param name="services">Service collection hvor CORS skal registreres</param>
  /// <param name="configuration">Configuration for å hente allowed origins</param>
  /// <returns>Den oppdaterte IServiceCollection-instansen for method chaining</returns>
  private static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
  {
      var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    
      if (allowedOrigins == null || allowedOrigins.Length == 0)
          throw new InvalidOperationException(
              "Cors:AllowedOrigins does not exists or is empty in appsettings.json. " +
              "Add atleast one allowed origin.");
    
      services.AddCors(options =>
      {
          options.AddPolicy("AllowBlazorApp", policy =>
          {
              policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
          });
      });
    
      return services;
  }

  /// <summary>
  /// Registrerer ASP.NET Core-tjenester (Controllers, Swagger, Exception Handling) Mer generell
  /// </summary>
  /// <param name="services">Service collection hvor tjenestene skal registreres</param>
  /// <returns>Den oppdaterte IServiceCollection-instansen for method chaining</returns>
  private static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
      // Controllers
      services.AddControllers();

    


      // Exception Handling
      services.AddExceptionHandler<GlobalExceptionHandler>();
      services.AddProblemDetails(options =>
      {
          options.CustomizeProblemDetails = context =>
          {
              context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
              context.ProblemDetails.Instance = context.HttpContext.Request.Path;
          };
      });





      // API Documentation
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen();
     
      return services;
  }




  /// <summary>
  /// Registrerer alle applikasjonsspesifikke tjenester og deres avhengigheter i DI-containeren
  /// Dette inkluderer repositories, services, og andre forretningslogikk-komponenter
  /// </summary>
  /// <param name="services">Service collection hvor tjenestene skal registreres</param>
  /// <param name="configuration">For å hente ut settings for database-stringen</param>
  /// <returns>Den oppdaterte IServiceCollection-instansen for method chaining</returns>
  private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
      // ========================================== DATABASE INITIALIZATION ==========================================
       // var connectionString = configuration.GetConnectionString("DefaultConnection")
       //                        ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.json");
       //
       // services.AddDbContext<AppDbContext>(options =>
       //     options.UseSqlite(connectionString));


        

      // Services
      services.AddScoped<IPasswordService, PasswordService>();
    
      // TODO: Repositories vil komme her
      // TODO: Validators vil komme her
    
      return services;
  }
}
