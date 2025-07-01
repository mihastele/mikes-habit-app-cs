using Microsoft.Maui.Controls;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MikeNet8HabitsApp.Pages;

public partial class HabitPerformancePage : ContentPage
{
    private readonly HabitPerformanceViewModel _viewModel;

    public HabitPerformancePage()
    {
        InitializeComponent();
        _viewModel = new HabitPerformanceViewModel(App.Current.Handler.MauiContext.Services.GetService<DatabaseService>(),
            App.Current.Handler.MauiContext.Services.GetService<SettingsService>());
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadData();
    }
}
