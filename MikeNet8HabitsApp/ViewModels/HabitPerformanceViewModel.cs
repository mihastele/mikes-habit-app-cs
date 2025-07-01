using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;
using Microsoft.Maui.Controls;

namespace MikeNet8HabitsApp.ViewModels;

public class HabitPerformanceViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private List<HabitPerformanceItem> _habitItems = new List<HabitPerformanceItem>();
    public List<HabitPerformanceItem> HabitItems 
    { 
        get => _habitItems;
        private set 
        { 
            _habitItems = value; 
            OnPropertyChanged(); 
        } 
    }

    // Overall metrics
    private int _totalCompleted;
    public int TotalCompleted { get => _totalCompleted; private set { _totalCompleted = value; OnPropertyChanged(); } }

    private double _avgPercentPerDay;
    public double AvgPercentPerDay { get => _avgPercentPerDay; private set { _avgPercentPerDay = value; OnPropertyChanged(); } }

    private int _allHabitsCompletedCount;
    public int AllHabitsCompletedCount { get => _allHabitsCompletedCount; private set { _allHabitsCompletedCount = value; OnPropertyChanged(); } }

    private int _successfulDaysCount;
    public int SuccessfulDaysCount { get => _successfulDaysCount; private set { _successfulDaysCount = value; OnPropertyChanged(); } }
    
    private int _currentAllHabitsStreak;
    public int CurrentAllHabitsStreak { get => _currentAllHabitsStreak; private set { _currentAllHabitsStreak = value; OnPropertyChanged(); } }
    
    private int _currentSuccessfulDayStreak;
    public int CurrentSuccessfulDayStreak { get => _currentSuccessfulDayStreak; private set { _currentSuccessfulDayStreak = value; OnPropertyChanged(); } }
    
    private int _longestAllHabitsStreak;
    public int LongestAllHabitsStreak { get => _longestAllHabitsStreak; private set { _longestAllHabitsStreak = value; OnPropertyChanged(); } }
    
    private int _longestSuccessfulDayStreak;
    public int LongestSuccessfulDayStreak { get => _longestSuccessfulDayStreak; private set { _longestSuccessfulDayStreak = value; OnPropertyChanged(); } }

    private readonly DatabaseService _db;
    private readonly SettingsService _settingsService;
    
    public int ThresholdPercent => _settingsService?.ThresholdPercent ?? 80;

    public HabitPerformanceViewModel(DatabaseService db, SettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        private set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsNotLoading => !IsLoading;

    private Command _loadDataCommand;
    public ICommand LoadDataCommand => _loadDataCommand ??= new Command(async () => await LoadDataAsync());

    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var habits = await _db.GetAllHabitsAsync();
            var habitItems = new List<HabitPerformanceItem>();
            
            foreach (var habit in habits)
            {
                var records = await _db.GetAllHabitRecordsForHabitAsync(habit.Id);
                int completedDays = records.Count(r => r.IsCompleted);
                double completionPercentage = records.Count > 0 ? (double)completedDays / records.Count * 100 : 0;
                int totalCompletions = completedDays;
                int currentStreak = CalculateCurrentStreak(records);
                int longestStreak = CalculateLongestStreak(records);
                
                habitItems.Add(new HabitPerformanceItem
                {
                    HabitName = habit.Name,
                    PerformanceStats = $"Completion: {completionPercentage:F2}%, Total Completions: {totalCompletions}, Current Streak: {currentStreak} days, Longest Streak: {longestStreak} days"
                });
            }

            HabitItems = habitItems;
            OnPropertyChanged(nameof(HabitItems));

            // Overall metrics calc
            var allRecords = await _db.GetAllHabitRecordsAsync();
            var groupedByDay = allRecords.GroupBy(r => r.Date.Date).OrderBy(g => g.Key).ToList();
            
            TotalCompleted = allRecords.Count(r => r.IsCompleted);
            
            // Calculate average percentage per day
            AvgPercentPerDay = groupedByDay.Any() && habits.Count > 0 
                ? groupedByDay.Average(g => (double)g.Count(r => r.IsCompleted) / habits.Count * 100) 
                : 0;
                
            // Calculate streaks and counts
            if (habits.Count > 0)
            {
                var allHabitsStreakInfo = await CalculateStreakInfoAsync(groupedByDay, habits.Count, requireAll: true);
                var successfulDayStreakInfo = await CalculateStreakInfoAsync(groupedByDay, habits.Count, requireAll: false);
                
                // All habits metrics
                AllHabitsCompletedCount = allHabitsStreakInfo.TotalDaysMet;
                CurrentAllHabitsStreak = allHabitsStreakInfo.CurrentStreak;
                LongestAllHabitsStreak = allHabitsStreakInfo.LongestStreak;
                
                // Successful days metrics
                SuccessfulDaysCount = successfulDayStreakInfo.TotalDaysMet;
                CurrentSuccessfulDayStreak = successfulDayStreakInfo.CurrentStreak;
                LongestSuccessfulDayStreak = successfulDayStreakInfo.LongestStreak;
            }
            else
            {
                AllHabitsCompletedCount = 0;
                CurrentAllHabitsStreak = 0;
                LongestAllHabitsStreak = 0;
                SuccessfulDaysCount = 0;
                CurrentSuccessfulDayStreak = 0;
                LongestSuccessfulDayStreak = 0;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading performance data: {ex.Message}";
            // Optionally log the full exception
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private int CalculateCurrentStreak(List<HabitRecord> records)
    {
        var sortedRecords = records.OrderByDescending(r => r.Date).ToList();
        int streak = 0;
        DateTime today = DateTime.Today;
        for (int i = 0; i < sortedRecords.Count; i++)
        {
            if (sortedRecords[i].IsCompleted && sortedRecords[i].Date.Date == today.AddDays(-streak))
            {
                streak++;
            }
            else if (streak > 0)
            {
                break;
            }
        }
        return streak;
    }

    private int CalculateLongestStreak(List<HabitRecord> records)
    {
        if (records.Count == 0) return 0;
        var sortedRecords = records.OrderBy(r => r.Date).ToList();
        int maxStreak = 0;
        int currentStreak = 1;
        for (int i = 1; i < sortedRecords.Count; i++)
        {
            if (sortedRecords[i].IsCompleted && sortedRecords[i].Date.Date.Subtract(sortedRecords[i - 1].Date.Date).Days == 1 && sortedRecords[i - 1].IsCompleted)
            {
                currentStreak++;
            }
            else if (sortedRecords[i].IsCompleted)
            {
                currentStreak = 1;
            }
            else
            {
                currentStreak = 0;
            }
            if (currentStreak > maxStreak) maxStreak = currentStreak;
        }
        return maxStreak;
    }

    private class StreakInfo
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalDaysMet { get; set; }
    }

    private async Task<StreakInfo> CalculateStreakInfoAsync(List<IGrouping<DateTime, HabitRecord>> days, int totalHabits, bool requireAll)
    {
        var result = new StreakInfo();
        if (days.Count == 0) return result;
        
        int currentStreak = 0;
        int longestStreak = 0;
        int totalDaysMet = 0;
        
        // Process days in chronological order
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        bool wasYesterdayProcessed = false;
        
        // Get all habit records for all habits to check yesterday's status
        Dictionary<DateTime, List<HabitRecord>> allRecordsByDate = new();
        if (days.Count > 0)
        {
            var allRecords = await _db.GetAllHabitRecordsAsync();
            allRecordsByDate = allRecords
                .GroupBy(r => r.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
        
        for (int i = 0; i < days.Count; i++)
        {
            var day = days[i];
            bool isSuccess = requireAll 
                ? day.Count(r => r.IsCompleted) == totalHabits
                : ((double)day.Count(r => r.IsCompleted) / totalHabits * 100) >= (_settingsService?.ThresholdPercent ?? 80);
            
            if (isSuccess)
            {
                totalDaysMet++;
                
                // Check if this is part of the current streak
                if (i == 0 || (i > 0 && (day.Key - days[i-1].Key).Days == 1))
                {
                    currentStreak++;
                }
                else if (i > 0 && (day.Key - days[i-1].Key).Days > 1)
                {
                    // There's a gap, reset current streak
                    currentStreak = 1;
                }
                
                // Update longest streak if current is longer
                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }
                
                // Track if we've processed yesterday for current streak calculation
                if (day.Key.Date == yesterday.Date)
                {
                    wasYesterdayProcessed = true;
                }
            }
            else
            {
                // Reset current streak if the day doesn't meet the criteria
                currentStreak = 0;
            }
        }
        
        // If we're checking today and it's a success, and yesterday wasn't processed, 
        // check if we should continue the streak from the last processed day
        var lastDay = days.Last().Key.Date;
        if (lastDay == today && !wasYesterdayProcessed && currentStreak > 0)
        {
            // Check if yesterday exists in our records
            if (allRecordsByDate.TryGetValue(yesterday, out var yesterdayRecords))
            {
                bool yesterdaySuccess = requireAll
                    ? yesterdayRecords.Count(r => r.IsCompleted) == totalHabits
                    : ((double)yesterdayRecords.Count(r => r.IsCompleted) / totalHabits * 100) >= (_settingsService?.ThresholdPercent ?? 80);
                
                if (!yesterdaySuccess)
                {
                    // If yesterday was a failure, reset current streak to 1 (just today)
                    currentStreak = 1;
                }
            }
        }
        
        result.CurrentStreak = currentStreak;
        result.LongestStreak = longestStreak;
        result.TotalDaysMet = totalDaysMet;
        
        return result;
    }
}

public class HabitPerformanceItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _habitName;
    public string HabitName
    {
        get => _habitName;
        set { _habitName = value; OnPropertyChanged(); }
    }

    private string _performanceStats;
    public string PerformanceStats
    {
        get => _performanceStats;
        set { _performanceStats = value; OnPropertyChanged(); }
    }
}
