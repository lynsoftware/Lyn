using Lyn.Backend.Apps.Calorie.Persistence;
using Lyn.Backend.Infrastructure.Persistence;
using Lyn.Backend.Startup;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ============================================== 1. BUILDER CONFIGURATION ==============================================
var builder = WebApplication.CreateBuilder(args);

// Konfigurer Kestrel limits og setter maks MB pr forespørsel
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});


// ============================================== 2. SERVICES ==============================================
builder.ConfigureServices();

// Konfigurer form options globalt
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// ============================================== 3. APP CONFIGURATION ==============================================
var app = builder.Build();

// Automatiserer databasemigrasjoner
using (var scope = app.Services.CreateScope())
{
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var calorieDb = scope.ServiceProvider.GetRequiredService<CalorieDbContext>();

    try
    {
        Log.Information("Applying database migrations...");
        await appDb.Database.MigrateAsync();
        await calorieDb.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while migrating the database");
        throw;
    }
}


// ============================================== 4. MIDDLEWARE PIPELINE & ENDPOINTS ==============================================
await app.SeedDatabaseAsync();
app.ConfigureMiddleware();


// ============================================== 5. RUN ==============================================
app.Run();

public partial class Program { }