using Amazon.Runtime;
using Amazon.S3;
using Lyn.Backend.Configuration.Options;
using Lyn.Backend.Data;
using Lyn.Backend.Middleware;
using Lyn.Backend.Models;
using Lyn.Backend.Repository;
using Lyn.Backend.Services;
using Lyn.Backend.Services.Interface;
using Lyn.Backend.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Resend;
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

      // AWS (S3)
      builder.AddAWS();
      
      // Database
      builder.Services.AddDatabase(builder.Configuration);
      
      // Authentication & Authorization
      builder.Services.AddIdentityAndAuthentication();
      
      // Application Services
      builder.Services.AddApplicationServices();
      
      // ReSend Email Service
      builder.Services.AddEmailService(builder.Configuration);
     
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
  /// Registrerer database context
  /// </summary>
  private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
      var connectionString = configuration.GetConnectionString("DefaultConnection")
                             ?? throw new InvalidOperationException("No DefaultConnection in appsettings.json");

      if (connectionString == "InMemory")
      {
          services.AddDbContext<AppDbContext>(options =>
              options.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}"));
      }
      else
      {
          services.AddDbContext<AppDbContext>(options =>
              options.UseNpgsql(connectionString));
      }
    
      return services;
  }
  
  /// <summary>
  /// Registrerer AWS (S3)
  /// </summary>
  private static WebApplicationBuilder AddAWS(this WebApplicationBuilder builder)
  {
      var awsOptions = builder.Configuration.GetAWSOptions();
      
      // Bruker appsetttings AccessKey/SecretKey til dev S3-bucket i development
      if (builder.Environment.IsDevelopment())
      {
          var accessKey = builder.Configuration["AWS:AccessKey"];
          var secretKey = builder.Configuration["AWS:SecretKey"];
        
          if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
          {
              awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
          }
      }
      
      builder.Services.AddDefaultAWSOptions(awsOptions); 
      builder.Services.AddAWSService<IAmazonS3>();
    
      return builder;
  }

  
  
  /// <summary>
  /// Configures JwtSettings, JwtService and sets up Authorization and AUthentication
  /// </summary>
  private static IServiceCollection AddIdentityAndAuthentication(this IServiceCollection services)
  {
      // Register og valider JwtSettings
      services.AddOptions<JwtSettings>()
          .BindConfiguration(JwtSettings.SectionName)
          .ValidateDataAnnotations()
          .ValidateOnStart();
     
      // Registerer JwtBearer options som vanligvis er i AddAuthenticaiton
      services.ConfigureOptions<ConfigureJwtBearerOptions>();
     
      // Registerer JwtService
      services.AddScoped<IJwtService, JwtService>();
     
     
      // Konfigurer Identity med vår egen bruker
      services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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
  /// Registrerer alle applikasjonsspesifikke tjenester og deres avhengigheter
  /// </summary>
  private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
      
      
      
      // Services
      services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<ISupportTicketService, SupportTicketService>();
      services.AddScoped<IReleaseService, ReleaseService>();
      services.AddScoped<IStorageService, S3StorageService>();
      
      
      
      
    
      // Repository
      services.AddScoped<IStatisticsRepository, StatisticsRepository>();
      services.AddScoped<ISupportRepository, SupportRepository>();
      services.AddScoped<IReleaseRepository, ReleaseRepository>();
      
      
      
      // Validators
      services.AddScoped<IFileValidator, FileValidator>();
      
      // Database Seeder
      services.AddScoped<DatabaseSeeder>();
    
      return services;
  }
}
