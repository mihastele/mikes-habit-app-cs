using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MikeNet8HabitsApp.Pages;

public partial class AddHabitPage : ContentPage
{
    private readonly DatabaseService _db;

    public AddHabitPage()
    {
        InitializeComponent();
        _db = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameEntry = (Entry)FindByName("NameEntry");
        var name = nameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Validation", "Please enter a habit name.", "OK");
            return;
        }

        var descriptionEntry = (Entry)FindByName("DescriptionEntry");
        var description = descriptionEntry.Text?.Trim();
        var countableSwitch = (Switch)FindByName("CountableSwitch");
        var isCountable = countableSwitch.IsToggled;

        Habit habit;
        if (isCountable)
        {
            var targetCountEntry = (Entry)FindByName("TargetCountEntry");
            int.TryParse(targetCountEntry.Text, out var target);
            habit = new CountableHabit
            {
                Name = name,
                Description = description,
                TargetCount = target,
                CurrentCount = 0,
                Color = Colors.LightGray,
                Streak = 0
            };
        }
        else
        {
            habit = new Habit
            {
                Name = name,
                Description = description,
                Color = Colors.LightGray,
                Streak = 0
            };
        }

        await _db.SaveHabitAsync(habit);
        await Navigation.PopAsync();
    }
}
