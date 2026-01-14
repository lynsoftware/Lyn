using Lyn.Backend.Configuration;
using Lyn.Backend.Data;
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
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        Log.Information("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
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