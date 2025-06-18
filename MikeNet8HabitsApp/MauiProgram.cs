using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikeNet8HabitsApp.ViewModels;

namespace MikeNet8HabitsApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<Services.DatabaseService>();
        builder.Services.AddSingleton<HabitPerformanceViewModel>();
        builder.Services.AddTransient<Pages.AddHabitPage>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        var databaseService = app.Services.GetService<Services.DatabaseService>();
        return app;
    }
}