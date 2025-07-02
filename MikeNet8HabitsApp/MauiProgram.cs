using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikeNet8HabitsApp.ViewModels;
using CommunityToolkit.Maui;

namespace MikeNet8HabitsApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<Services.DatabaseService>();
        builder.Services.AddSingleton<Services.SettingsService>();

        builder.Services.AddSingleton<ViewModels.HabitPerformanceViewModel>();

        // Pages
        builder.Services.AddTransient<Pages.AddHabitPage>();
        builder.Services.AddTransient<Pages.CalendarPage>();
        builder.Services.AddTransient<Pages.SettingsPage>();
        builder.Services.AddTransient<Pages.HabitPerformancePage>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        
        // Initialize database before the app starts
        var databaseService = app.Services.GetRequiredService<Services.DatabaseService>();
        var initTask = databaseService.InitializeAsync();
        initTask.Wait(); // Block here to ensure DB is initialized before proceeding
        
        if (initTask.IsFaulted)
        {
            // Log the error and crash the app if we can't initialize the database
            System.Diagnostics.Debug.WriteLine($"Failed to initialize database: {initTask.Exception}");
            throw initTask.Exception;
        }
        
        return app;
    }
}