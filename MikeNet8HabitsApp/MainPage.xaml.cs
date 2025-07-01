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
        private bool _isLoadingHabits = false;

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
            _isLoadingHabits = true; // Set flag to ignore checkbox changes
            try
            {
                await LoadHabitsAsync(); // Load all habits first
                foreach (var habit in _habits)
                {
                    var record = await _db.GetHabitRecordAsync(habit.Id, date);
                    // var firstRecord = await _db.DebugHabitRecordAsync();
                    // DisplayAlert("Record", record?.Id + " " + record?.Date + " " + record?.IsCompleted, "OK");
                    // DisplayAlert("Record present?", firstRecord?.Id + " " + firstRecord?.HabitId + " " + firstRecord?.Date + " " + firstRecord?.IsCompleted, "OK");
                    habit.IsCompleted =
                        record?.IsCompleted ?? false; // Set IsCompleted from HabitRecord or default to false
                }
            }
            finally
            {
                _isLoadingHabits = false; // Reset flag when done
            }
            // await LoadHabitsAsync(); // Load all habits first
            // foreach (var habit in _habits)
            // {
            //     var record = await _db.GetHabitRecordAsync(habit.Id, date);
            //     var firstRecord = await _db.DebugHabitRecordAsync();
            //     DisplayAlert("Record", record?.Id + " " + record?.Date + " " + record?.IsCompleted, "OK");
            //     DisplayAlert("Record present?", firstRecord?.Id + " " + firstRecord?.HabitId + " " + firstRecord?.Date + " " + firstRecord?.IsCompleted, "OK");
            //     habit.IsCompleted = record?.IsCompleted ?? false; // Set IsCompleted from HabitRecord or default to false
            // }
            // // UpdateHabitsList(_habits); // Load all habits first
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_db != null)
            {
                await _db.InitializeAsync();
            }

            await LoadHabitsAsync();
        }

        private async void OnHabitCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isLoadingHabits) // Skip processing if we're just loading habits
                return;

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

        // private async void OnHomeClicked(object sender, EventArgs e)
        // {
        //     await Navigation.PushAsync(new MainPage());
        //     Navigation.RemovePage(this);
        // }
        private async void OnAddHabitClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.AddHabitPage());
            // Navigation.RemovePage(this);
        }

        private async void OnProgressClicked(object sender, EventArgs e)
        {
            // await Navigation.PopAsync();
            await Navigation.PushAsync(new Pages.HabitPerformancePage());
            // Navigation.RemovePage(this);
        }

        private async void OnCalendarClicked(object sender, EventArgs e)
        {
            // await Navigation.PopAsync();
            await Navigation.PushAsync(new Pages.CalendarPage());
            // Navigation.RemovePage(this);
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
            await Navigation.PushAsync(new Pages.SettingsPage());
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
            // Ensure we're working with date-only
            var dateForRecord = _currentDate.Date;

            var record = await _db.GetHabitRecordAsync(habit.Id, dateForRecord);
            // DisplayAlert("Record", record?.Id + " " + record?.Date + " " + record?.IsCompleted, "OK");
            
            // // Debug the record we found (or didn't find)
            // string recordInfo = record == null 
            //     ? "No existing record found" 
            //     : $"Found record: ID={record.Id}, Date={record.Date:yyyy-MM-dd}, IsCompleted={record.IsCompleted}";
            // await DisplayAlert("Record Info", recordInfo, "OK");
            
            if (record == null)
            {
                record = new HabitRecord
                {
                    HabitId = habit.Id,
                    Date = dateForRecord, // Store only the date part
                    IsCompleted = habit.IsCompleted
                };
                await _db.SaveHabitRecordAsync(record);
            }
            else
            {
                record.IsCompleted = habit.IsCompleted; // Update existing record
                await _db.SaveHabitRecordAsync(record);
            }
            
            // await LoadHabitsAsync();
        }

        private async void OnDeleteHabitClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Habit habit)
            {
                var confirm = await DisplayAlert("Confirm Deletion", $"Are you sure you want to delete '{habit.Name}'?",
                    "Yes", "No");
                if (confirm)
                {
                    await _db.DeleteHabitAsync(habit.Id);
                    await LoadHabitsAsync(); // Refresh the list after deletion
                }
            }
        }
    }
}