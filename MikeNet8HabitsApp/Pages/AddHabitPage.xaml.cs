using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Switch = Microsoft.Maui.Controls.Switch;

namespace MikeNet8HabitsApp.Pages;

public partial class AddHabitPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _isSaving = false;

    public AddHabitPage()
    {
        try
        {
            InitializeComponent();
            _db = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>() ?? 
                  throw new InvalidOperationException("Unable to resolve database service");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Page initialization failed: {ex}");
            throw;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isSaving) return; // Prevent multiple clicks
        
        try
        {
            _isSaving = true;
            
            var nameEntry = (Entry)FindByName("NameEntry");
            var name = nameEntry?.Text?.Trim() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Validation", "Please enter a habit name.", "OK");
                return;
            }

            var descriptionEntry = (Entry)FindByName("DescriptionEntry");
            var description = descriptionEntry?.Text?.Trim() ?? string.Empty;
            var countableSwitch = (Switch)FindByName("CountableSwitch");
            var isCountable = countableSwitch?.IsToggled ?? false;

            var targetCount = 0;
            if (isCountable)
            {
                var targetCountEntry = (Entry)FindByName("TargetCountEntry");
                if (!int.TryParse(targetCountEntry?.Text, out targetCount) || targetCount <= 0)
                {
                    await DisplayAlert("Validation", "Please enter a valid target count greater than zero.", "OK");
                    return;
                }
            }
            
            var habit = new Habit
            {
                Name = name,
                Description = description,
                IsCountable = isCountable,
                TargetCount = targetCount,
                CurrentCount = 0,
                ColorHex = Colors.LightGray.ToHex(),
                Streak = 0,
                IsCompleted = false
            };


            Debug.WriteLine($"Attempting to save habit: {habit.Name}");
            await _db.SaveHabitAsync(habit);
            Debug.WriteLine($"Habit saved successfully: {habit.Name}");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving habit: {ex}");
            await DisplayAlert("Error", "An error occurred while saving the habit. Please try again.", "OK");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
