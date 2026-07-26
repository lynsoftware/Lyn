using Calorie.Common;
using Calorie.Common.Services;
using Calorie.Core.Common;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.DailyLog.Services;
using Calorie.Core.Features.DailyLog.ViewModels;
using Calorie.Core.Features.Goals.Services;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Features.Library.ViewModels;
using Calorie.Core.Features.MainPage.ViewModels;
using Calorie.Core.Features.Stats.Services;
using Calorie.Features.DailyLog.Pages;
using Calorie.Features.Library.Pages;
using Calorie.Features.Settings.Pages;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;


namespace Calorie;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.WriteLine($"UNHANDLED: {args.ExceptionObject}");
        };

        // Lokal SQLite i appens sandkasse — migreres og seedes ved oppstart
        Calorie.Core.Data.LocalDb.Initialize(Path.Combine(FileSystem.AppDataDirectory, "calorie.db3"));
        
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontAwesome");
                fonts.AddFont("Electrolize-Regular.ttf", "Electrolize");
            });
        
        // Delte tjenester for ViewModels — én instans for hele appen
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IToastService, ToastService>();
        builder.Services.AddSingleton<IAppearanceService, AppearanceService>();

        // Klokka bak all "i dag"-logikk — testene bytter til FakeTimeProvider
        builder.Services.AddSingleton(TimeProvider.System);

        // Storene — tilstandsløse SQLite-fasader bak interfaces (4B-2 pkt. 8)
        builder.Services.AddSingleton<ILibraryStore, LibraryStore>();
        builder.Services.AddSingleton<IGoalStore, GoalStore>();
        builder.Services.AddSingleton<IDayLogStore, DayLogStore>();
        builder.Services.AddSingleton<IStatsStore, StatsStore>();

        // Sider + ViewModels — transient: fersk instans per navigering
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Core.Features.Settings.ViewModels.SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<Core.Features.Goals.ViewModels.GoalSettingsViewModel>();
        builder.Services.AddTransient<Features.Goals.Pages.GoalSettingsPage>();
        builder.Services.AddTransient<AddLogEntryViewModel>();
        builder.Services.AddTransient<AddLogEntryPage>();
        builder.Services.AddTransient<IngredientEditorViewModel>();
        builder.Services.AddTransient<IngredientEditorPage>();
        builder.Services.AddTransient<MealBuilderViewModel>();
        builder.Services.AddTransient<MealBuilderPage>();
        builder.Services.AddTransient<LibraryPageViewModel>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<Core.Features.Stats.ViewModels.StatsViewModel>();
        builder.Services.AddTransient<Features.Stats.Pages.StatsPage>();

        // Shell-ruter — sider man kan navigere til med GoToAsync
        Routing.RegisterRoute(AppRoutes.AddLogEntry, typeof(AddLogEntryPage));
        Routing.RegisterRoute(AppRoutes.IngredientEditor, typeof(IngredientEditorPage));
        Routing.RegisterRoute(AppRoutes.MealBuilder, typeof(MealBuilderPage));
        Routing.RegisterRoute(AppRoutes.Library, typeof(LibraryPage));
        Routing.RegisterRoute(AppRoutes.Stats, typeof(Features.Stats.Pages.StatsPage));
        Routing.RegisterRoute(AppRoutes.Settings, typeof(SettingsPage));
        Routing.RegisterRoute(AppRoutes.GoalSettings, typeof(Features.Goals.Pages.GoalSettingsPage));
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}