using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;

namespace MikeNet8HabitsApp.Pages;

public partial class AddHabitPage : ContentPage
{
    private readonly DatabaseService _database;

    public AddHabitPage()
    {
        InitializeComponent();
        _database = App.Current?.Services.GetService(typeof(DatabaseService)) as DatabaseService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Validation", "Please enter a habit name.", "OK");
            return;
        }

        var description = DescriptionEntry.Text?.Trim();
        var isCountable = CountableSwitch.IsToggled;

        Habit habit;
        if (isCountable)
        {
            int.TryParse(TargetCountEntry.Text, out var target);
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

        await _database.SaveHabitAsync(habit);
        await Navigation.PopAsync();
    }
}
