using Amazon.Runtime;
using Amazon.S3;
using Lyn.Backend.Infrastructure.Email;
using Lyn.Backend.Infrastructure.Files.Validators;
using Lyn.Backend.Infrastructure.Middleware;
using Lyn.Backend.Infrastructure.Persistence;
using Lyn.Backend.Infrastructure.Persistence.Options;
using Lyn.Backend.Infrastructure.Persistence.Seeders;
using Lyn.Backend.Infrastructure.Storage.Services;
using Lyn.Backend.Platform.Auth.Models;
using Lyn.Backend.Platform.Auth.Options;
using Lyn.Backend.Startup.Modules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Resend;
using Serilog;

namespace Lyn.Backend.Startup;

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
      builder.Services
          .AddPersistence(builder.Configuration)
          .AddEmailService(builder.Configuration)
          .AddApplicationServices()
          .AddAuthInfrastructure()
          .AddStorage(builder.Configuration, builder.Environment);
      
      // Adds platform specific features, common and modules for the apps
      builder.Services.AddPlatform()
          .AddFileValidation()
          .AddPasswordGenerator()
          .AddCalorieModule();
      
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
  /// Registrerer database context
  /// </summary>
  private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
  {
      services.AddOptions<DatabaseSettings>()
          .BindConfiguration(DatabaseSettings.SectionName)
          .ValidateDataAnnotations()
          .ValidateOnStart();

      services.AddDbContext<AppDbContext>((sp, options) =>
      {
          var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
          options.UseNpgsql(settings.DefaultConnection);
      });
    
      return services;
  }
  
  /// <summary>
  /// Registrerer AWS (S3)
  /// </summary>
  private static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration, 
      IWebHostEnvironment environment)
  {
      var awsOptions = configuration.GetAWSOptions();
      
      // Bruker appsetttings AccessKey/SecretKey til dev S3-bucket i development
      if (environment.IsDevelopment())
      {
          var accessKey = configuration["AWS:AccessKey"];
          var secretKey = configuration["AWS:SecretKey"];
        
          if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
          {
              awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
          }
      }
      
      services.AddDefaultAWSOptions(awsOptions); 
      services.AddAWSService<IAmazonS3>();
      
      // Services
      services.AddScoped<IStorageService, S3StorageService>();
    
      return services;
  }

  
  
  /// <summary>
  /// Configures JwtSettings, JwtService and sets up Authorization and AUthentication
  /// </summary>
  private static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
  {
      // Register og valider JwtSettings
      services.AddOptions<JwtSettings>()
          .BindConfiguration(JwtSettings.SectionName)
          .ValidateDataAnnotations()
          .ValidateOnStart();
     
      // Registerer JwtBearer options som vanligvis er i AddAuthenticaiton
      services.ConfigureOptions<ConfigureJwtBearerOptions>();
     
     
      // Konfigurer Identity med vår egen bruker
      services.AddIdentity<AppUser, IdentityRole>(options =>
          {
              options.Password.RequireDigit = true;
              options.Password.RequireLowercase = true;
              options.Password.RequireUppercase = true;
              options.Password.RequireNonAlphanumeric = false;
              options.Password.RequiredLength = 8;


              options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
              options.Lockout.MaxFailedAccessAttempts = 5;
              options.Lockout.AllowedForNewUsers = true;


              options.User.RequireUniqueEmail = true;
          })
          .AddEntityFrameworkStores<AppDbContext>()
          .AddDefaultTokenProviders();
     
     
     
      // Her forteller vi ASP.NET Core at vi skal bruke JWT-tokens for autentisering
      services.AddAuthentication(options =>
      {
          // Dette sikrer at vi sjekker JWT automatisk når vi får inn en request
          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          // Hvis brukeren ikke har med token så vå de en 401 Unathorized
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          // Dette er bare backup og brukes hvis ikke noe annet er spesifisert. Feks hvis det er flere type
          // autentiseringsmuligheter i appen så kan man velge hvem det skal falles tilbake på
          options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      }).AddJwtBearer();

        
      services.AddAuthorization();
      
      // Database Seeder
      services.AddScoped<DatabaseSeeder>();
      
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
      services.AddSwaggerGen(options =>
      { 
          // Lager en jwtSecurityScheme
          var jwtSecurityScheme = new OpenApiSecurityScheme
          {
              // Setter det at bearer skal inneholde JWT
              BearerFormat = "JWT",
              // Egendefinert navn som vises i UI-en
              Name = "JWT Authorization",
              // Hvilken type autentiseringsmekanisme vi skal bruke, feks Http, ApiKey
              Type = SecuritySchemeType.Http,
              // Forteller hvilket scheme vi skal bruke, og JwtBearerDefaults.AuthenticationScheme = "Bearer"
              Scheme = JwtBearerDefaults.AuthenticationScheme,
              // Egendefinert beskrivelse som vises i UI-en
              Description = "Enter your JWT Access Token",
              // Vi finner tokenet i headeren
              In = ParameterLocation.Header,
              // Lager en refereanse slik at alle endepunkter med [Authorize] refe rer til samme oppsett, eller så fyller
              // det seg opp med slike oppsett pr endepunkt
          };


          // Vi registerer oppsettet med Bearer og OpenApiSecurityScheme objeektet vårt
          options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);


          // Dette forteller Swagger at alle endepunkter med [Authorize]-attributen bruker JWT
          options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
          {
              {
                  new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document),
                  []
              }
          });
          
          // API Key for GitHub release uploads
          options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
          {
              Name = "X-Api-Key",
              Type = SecuritySchemeType.ApiKey,
              In = ParameterLocation.Header,
              Description = "API Key for release uploads (GitHub Actions)"
          });

          options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
          {
              {
                  new OpenApiSecuritySchemeReference("ApiKey", document),
                  []
              }
          });
      });
      
      return services;
  }
  
  /// <summary>
  /// Add ReSend Email service
  /// </summary>
  private static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
  {
      // Email Service Resend
      var resendApiKey = configuration["Resend:ApiKey"]
                         ?? throw new InvalidOperationException(
                             "Resend:ApiKey is not configured. " +
                             "Add RESEND_API_KEY to environment variables or appsettings.json");
      
      // Register Resend options
      services.Configure<ResendClientOptions>(o => o.ApiToken = resendApiKey);
    
      // HttpClient for Resend
      services.AddHttpClient<IResend, ResendClient>();
       
    
      services.AddScoped<IEmailService, EmailService>();
      
      return services;
  }



  /// <summary>
  /// Registrerer filvaldiering
  /// </summary>
  private static IServiceCollection AddFileValidation(this IServiceCollection services)
  {
      services.AddScoped<IFileValidator, FileValidator>();
      return services;
  }
}
