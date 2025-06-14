﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using MikeNet8HabitsApp.Classes;

namespace MikeNet8HabitsApp
{
    public partial class MainPage : ContentPage
    {
        private DateTime _currentDate = DateTime.Today;
        private ObservableCollection<Habit> _habits;
        private Random _random = new Random();

        public MainPage()
        {
            InitializeComponent();

            // Initialize habits collection
            _habits = new ObservableCollection<Habit>();

            // Add some sample habits for demonstration
            _habits.Add(new Habit { Name = "Exercise", Description = "30 minutes", Streak = 3 });
            _habits.Add(new Habit { Name = "Read", Description = "20 pages", Streak = 7 });
            _habits.Add(new CountableHabit
            {
                Name = "Drink Water", Description = "6 glasses a day", Streak = 2, TargetCount = 6, CurrentCount = 0
            });

            // Set the collection as the source for the CollectionView
            HabitsCollection.ItemsSource = _habits;

            // Update the date display
            UpdateDateDisplay();
        }

        private void UpdateDateDisplay()
        {
            string dateFormat = _currentDate.Date == DateTime.Today.Date
                ? "Today, MMMM d"
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

        private void LoadHabitsForDate(DateTime date)
        {
            // In a real app, this would load habits from a database for the specified date
            // For now, we'll just simulate it by clearing and re-adding items
            _habits.Clear();

            // Add sample habits with random completion status
            _habits.Add(new Habit
            {
                Name = "Exercise",
                Description = "30 minutes",
                Streak = 3,
                IsCompleted = date < DateTime.Today && _random.Next(2) == 0
            });
            _habits.Add(new Habit
            {
                Name = "Read",
                Description = "20 pages",
                Streak = 7,
                IsCompleted = date < DateTime.Today && _random.Next(2) == 0
            });
            _habits.Add(new CountableHabit
            {
                Name = "Drink Water",
                Description = "6 glasses a day",
                Streak = 2,
                TargetCount = 6,
                CurrentCount = _random.Next(6),
                IsCompleted = date < DateTime.Today && _random.Next(2) == 0
            });
        }

        private void OnHabitCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is Habit habit)
            {
                // Update the habit's completion status
                habit.IsCompleted = e.Value;

                // In a real app, you would save this change to a database
                // SaveHabitCompletionStatus(habit, _currentDate, e.Value);
            }
        }

        private void OnAddHabitClicked(object sender, EventArgs e)
        {
            // Navigate to the Add Habit page
            DisplayAlert("Add Habit", "Navigate to Add Habit page", "OK");
            // In a real app: await Navigation.PushAsync(new AddHabitPage());
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

        private void OnIncrementCountClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CountableHabit habit)
            {
                habit.CurrentCount++;
                if (habit.CurrentCount >= habit.TargetCount)
                {
                    habit.IsCompleted = true;
                }

                // Force UI refresh
                HabitsCollection.ItemsSource = null;
                HabitsCollection.ItemsSource = _habits;
            }
        }
    }
}