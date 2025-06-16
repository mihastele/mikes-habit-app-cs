using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MikeNet8HabitsApp
{
    public partial class MainPage : ContentPage
    {
        private DateTime _currentDate = DateTime.Today;
        private ObservableCollection<Habit> _habits;
        private readonly Services.DatabaseService _db;

        public MainPage()
        {
            InitializeComponent();

            _db = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>();
            _habits = new ObservableCollection<Habit>();
            HabitsCollection.ItemsSource = _habits;

            UpdateDateDisplay();
        }

        private void UpdateDateDisplay()
        {
            string dateFormat = _currentDate.Date == DateTime.Today.Date
                ? $"Today, {_currentDate:MMMM d}"
                : _currentDate.ToString("dddd, MMMM d");
            CurrentDateLabel.Text = dateFormat;
        }

        private void OnPreviousDayClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(-1);
            UpdateDateDisplay();
            // Here you would load habits for the selected date
            LoadHabitsForDate(_currentDate);
        }

        private void OnNextDayClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(1);
            UpdateDateDisplay();
            // Here you would load habits for the selected date
            LoadHabitsForDate(_currentDate);
        }

        private async Task LoadHabitsAsync()
        {
            var list = await _db.GetAllHabitsAsync();
            _habits.Clear();
            foreach (var h in list)
            {
                _habits.Add(h);
            }
        }

        private async void LoadHabitsForDate(DateTime date)
        {
            // For this simple version, we ignore date filtering and just reload all habits.
            await LoadHabitsAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHabitsAsync();
        }

        private async void OnHabitCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is Habit habit)
            {
                // Update the habit's completion status
                habit.IsCompleted = e.Value;

                await _db.SaveHabitAsync(habit);
            }
        }

        private async void OnAddHabitClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.AddHabitPage());
        }

        private void OnProgressClicked(object sender, EventArgs e)
        {
            // Navigate to the Progress page
            DisplayAlert("Progress", "Navigate to Progress page", "OK");
            // In a real app: await Navigation.PushAsync(new ProgressPage());
        }

        private void OnCalendarClicked(object sender, EventArgs e)
        {
            // Navigate to the Calendar page
            DisplayAlert("Calendar", "Navigate to Calendar page", "OK");
            // In a real app: await Navigation.PushAsync(new CalendarPage());
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            // Navigate to the Settings page
            DisplayAlert("Settings", "Navigate to Settings page", "OK");
            // In a real app: await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnIncrementCountClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CountableHabit habit)
            {
                habit.CurrentCount++;
                if (habit.CurrentCount >= habit.TargetCount)
                {
                    habit.IsCompleted = true;
                }

                await _db.SaveHabitAsync(habit);

                // Force UI refresh
                await LoadHabitsAsync();
            }
        }
    }
}