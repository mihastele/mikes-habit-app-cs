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
            
            LoadHabitsForDate(_currentDate);
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
        
        // private void UpdateHabitsList(List<Habit> list)
        // {
        //     // var list = await _db.GetAllHabitsAsync();
        //     _habits.Clear();
        //     foreach (var h in list)
        //     {
        //         _habits.Add(h);
        //     }
        // }

        private async void LoadHabitsForDate(DateTime date)
        {
            await LoadHabitsAsync(); // Load all habits first
            foreach (var habit in _habits)
            {
                var record = await _db.GetHabitRecordAsync(habit.Id, date);
                var firstRecord = await _db.DebugHabitRecordAsync();
                DisplayAlert("Record", record?.Id + " " + record?.Date + " " + record?.IsCompleted, "OK");
                DisplayAlert("Record present?", firstRecord?.Id + " " + firstRecord?.HabitId + " " + firstRecord?.Date + " " + firstRecord?.IsCompleted, "OK");
                habit.IsCompleted = record?.IsCompleted ?? false; // Set IsCompleted from HabitRecord or default to false
            }
            // UpdateHabitsList(_habits); // Load all habits first
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(_db != null)
            {
                await _db.InitializeAsync();
            }
            await LoadHabitsAsync();
        }

        private async void OnHabitCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is Habit habit)
            {
                habit.IsCompleted = e.Value;
                if (habit is CountableHabit countableHabit && !e.Value)
                {
                    countableHabit.CurrentCount = 0;
                }
                await _db.SaveHabitAsync(habit);
                await UpdateHabitRecord(habit);
                // var record = await _db.GetHabitRecordAsync(habit.Id, _currentDate);
                // if (record == null)
                // {
                //     record = new HabitRecord { HabitId = habit.Id, Date = _currentDate, IsCompleted = habit.IsCompleted };
                //     await _db.SaveHabitRecordAsync(record);
                // }
                // else
                // {
                //     record.IsCompleted = habit.IsCompleted;
                //     await _db.SaveHabitRecordAsync(record);
                // }
                // await LoadHabitsAsync();  // Refresh UI
            }
        }

        private async void OnAddHabitClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.AddHabitPage());
        }

        private async void OnProgressClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.HabitPerformancePage());
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
                await UpdateHabitRecord(habit);
            }
        }
        
        async Task UpdateHabitRecord(Habit habit)
        {
            var record = await _db.GetHabitRecordAsync(habit.Id, _currentDate);
            if (record == null)
            {
                record = new HabitRecord 
                { 
                    HabitId = habit.Id, 
                    Date = _currentDate, 
                    IsCompleted = habit.IsCompleted 
                };
                await _db.SaveHabitRecordAsync(record);
            }
            else
            {
                record.IsCompleted = habit.IsCompleted;  // Update existing record
                await _db.SaveHabitRecordAsync(record);
            }
            await LoadHabitsAsync();  // Refresh UI
        }

        private async void OnDeleteHabitClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Habit habit)
            {
                var confirm = await DisplayAlert("Confirm Deletion", $"Are you sure you want to delete '{habit.Name}'?", "Yes", "No");
                if (confirm)
                {
                    await _db.DeleteHabitAsync(habit.Id);
                    await LoadHabitsAsync();  // Refresh the list after deletion
                }
            }
        }
    }
}