using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MikeNet8HabitsApp.Classes;
using MikeNet8HabitsApp.Services;

namespace MikeNet8HabitsApp.ViewModels;

public class HabitPerformanceViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public List<HabitPerformanceItem> HabitItems { get; private set; } = new List<HabitPerformanceItem>();

    // Overall metrics
    private int _totalCompleted;
    public int TotalCompleted { get => _totalCompleted; private set { _totalCompleted = value; OnPropertyChanged(); } }

    private double _avgPercentPerDay;
    public double AvgPercentPerDay { get => _avgPercentPerDay; private set { _avgPercentPerDay = value; OnPropertyChanged(); } }

    private int _allHabitsStreak;
    public int AllHabitsStreak { get => _allHabitsStreak; private set { _allHabitsStreak = value; OnPropertyChanged(); } }

    private int _successfulDayStreak;
    public int SuccessfulDayStreak { get => _successfulDayStreak; private set { _successfulDayStreak = value; OnPropertyChanged(); } }

    private readonly DatabaseService _db;
    private readonly SettingsService _settingsService;

    public HabitPerformanceViewModel(DatabaseService db, SettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }

    public async void LoadData()
    {
        var habits = await _db.GetAllHabitsAsync();
        HabitItems.Clear();
        foreach (var habit in habits)
        {
            var records = await _db.GetAllHabitRecordsForHabitAsync(habit.Id);
            int completedDays = records.Count(r => r.IsCompleted);
            double completionPercentage = records.Count > 0 ? (double)completedDays / records.Count * 100 : 0;
            int totalCompletions = completedDays;
            int currentStreak = CalculateCurrentStreak(records);
            int longestStreak = CalculateLongestStreak(records);
            HabitItems.Add(new HabitPerformanceItem
            {
                HabitName = habit.Name,
                PerformanceStats = $"Completion: {completionPercentage:F2}%, Total Completions: {totalCompletions}, Current Streak: {currentStreak} days, Longest Streak: {longestStreak} days"
            });
        }
        OnPropertyChanged(nameof(HabitItems));

        // Overall metrics calc
        var allRecords = await _db.GetAllHabitRecordsAsync();
        TotalCompleted = allRecords.Count(r => r.IsCompleted);

        var grouped = allRecords.GroupBy(r => r.Date.Date);
        AvgPercentPerDay = grouped.Any() ? grouped.Average(g => (double)g.Count(r => r.IsCompleted) / habits.Count * 100) : 0;

        AllHabitsStreak = CalculateStreak(grouped, habits.Count, requireAll: true);
        SuccessfulDayStreak = CalculateStreak(grouped, habits.Count, requireAll: false);
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

    private int CalculateStreak(IEnumerable<IGrouping<DateTime, HabitRecord>> groupByDay, int totalHabits, bool requireAll)
    {
        int streak = 0, max = 0;
        var ordered = groupByDay.OrderBy(g => g.Key).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            bool success = requireAll ? ordered[i].Count(r => r.IsCompleted) == totalHabits
                                       : ((double)ordered[i].Count(r => r.IsCompleted) / totalHabits * 100) >= (_settingsService?.ThresholdPercent ?? 80);
            if (success)
            {
                streak++;
                max = Math.Max(max, streak);
            }
            else streak = 0;
        }
        return max;
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
