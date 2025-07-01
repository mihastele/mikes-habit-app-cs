using System.Collections.ObjectModel;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.Classes;

namespace MikeNet8HabitsApp.ViewModels;

public class CalendarViewModel
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;
    public ObservableCollection<CalendarDay> Days { get; } = new();

    public CalendarViewModel(DatabaseService db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async void Load(DateTime monthDate)
    {
        Days.Clear();
        var first = new DateTime(monthDate.Year, monthDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(first.Year, first.Month);
        var records = await _db.GetAllHabitRecordsAsync();
        var habits = await _db.GetAllHabitsAsync();
        for (int d = 1; d <= daysInMonth; d++)
        {
            var date = first.AddDays(d - 1);
            var dayRecords = records.Where(r => r.Date.Date == date.Date).ToList();
            int completed = dayRecords.Count(r => r.IsCompleted);
            int total = habits.Count;
            double percent = total == 0 ? 0 : (double)completed / total * 100;
            bool success = percent >= _settings.ThresholdPercent;
            Days.Add(new CalendarDay
            {
                Date = date,
                DisplayText = success ? "✓" : "✗",
                BgColor = success ? Colors.LightGreen : Colors.LightPink
            });
        }
    }
}

public class CalendarDay
{
    public DateTime Date { get; set; }
    public string DisplayText { get; set; }
    public Color BgColor { get; set; }
}
