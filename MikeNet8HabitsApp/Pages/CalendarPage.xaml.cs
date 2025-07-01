using Microsoft.Maui.Controls;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.ViewModels;

namespace MikeNet8HabitsApp.Pages;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _vm;
    private DateTime _currentMonth;
    public CalendarPage()
    {
        InitializeComponent();
        var db = App.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
        var settings = App.Current.Handler.MauiContext.Services.GetService<SettingsService>();
        _vm = new CalendarViewModel(db, settings);
        BindingContext = _vm;
        _currentMonth = DateTime.Today;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load(_currentMonth);
    }

    private void PrevMonth_Clicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        _vm.Load(_currentMonth);
    }

    private void NextMonth_Clicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        _vm.Load(_currentMonth);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (CalendarCollection?.ItemsLayout is GridItemsLayout grid)
        {
            grid.Span = width < height ? 4 : 7; // fewer columns in portrait
        }
    }
}
