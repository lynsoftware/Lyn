using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Lyn.Web;
using Lyn.Web.Services;
using Serilog;
using Serilog.Events;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ================================== Logging ==================================
// Sett logging-nivå basert på miljø
var minimumLevel = builder.HostEnvironment.IsDevelopment() 
    ? LogEventLevel.Debug 
    : LogEventLevel.Warning;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(minimumLevel)
    .WriteTo.BrowserConsole(
        restrictedToMinimumLevel: minimumLevel,
        outputTemplate: builder.HostEnvironment.IsDevelopment()
            ? "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            : "[{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .Enrich.FromLogContext()
    .CreateLogger();

// ================================== Services ==================================
builder.Services.AddBlazorBootstrap();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization();

    

builder.Services.AddScoped<IPasswordGenerationService, PasswordGenerationService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();


// HttpClient with API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
                 ?? throw new InvalidOperationException("ApiBaseUrl not configured in appsettings.json");

builder.Services.AddScoped(_ => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl)
});


try
{
    Log.Information("Starting Blazor WebAssembly application in {Environment} mode", 
        builder.HostEnvironment.Environment);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
    
    // Etter builder.Build()
    var host = builder.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}