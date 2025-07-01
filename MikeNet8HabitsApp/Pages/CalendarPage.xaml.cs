using Microsoft.Maui.Controls;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.ViewModels;

namespace MikeNet8HabitsApp.Pages;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _vm;
    public CalendarPage()
    {
        InitializeComponent();
        var db = App.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
        var settings = App.Current.Handler.MauiContext.Services.GetService<SettingsService>();
        _vm = new CalendarViewModel(db, settings);
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load(DateTime.Today);
    }
}
