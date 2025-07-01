using System.Collections.ObjectModel;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.Classes;
using System.ComponentModel;

namespace MikeNet8HabitsApp.ViewModels;

public class CalendarViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;
    public ObservableCollection<CalendarDay> Days { get; } = new();
    public string MonthTitle
    {
        get => _monthTitle;
        private set
        {
            if (_monthTitle == value) return;
            _monthTitle = value;
            OnPropertyChanged(nameof(MonthTitle));
        }
    }
    private string _monthTitle;

    public CalendarViewModel(DatabaseService db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async void Load(DateTime monthDate)
    {
        Days.Clear();
        MonthTitle = monthDate.ToString("MMMM yyyy");
        var first = new DateTime(monthDate.Year, monthDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(first.Year, first.Month);
        var records = await _db.GetAllHabitRecordsAsync();
        var habits = await _db.GetAllHabitsAsync();
        for (int d = 1; d <= daysInMonth; d++)
        {
            var date = first.AddDays(d - 1);
            var dayAbbrev = date.ToString("ddd");//.ToLower();
            var displayBase = $"{d}. ({dayAbbrev})";
            if (date.Date > DateTime.Today)
            {
                // Future day, show placeholder only
                Days.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day.ToString(),
                    DisplayText = displayBase,
                    StatusIcon = string.Empty,
                    StatusColor = Colors.LightGray
                });
                continue;
            }
            var dayRecords = records.Where(r => r.Date.Date == date.Date).ToList();
            int completed = dayRecords.Count(r => r.IsCompleted);
            int total = habits.Count;
            double percent = total == 0 ? 0 : (double)completed / total * 100;
            bool success = percent >= _settings.ThresholdPercent;
            Days.Add(new CalendarDay
            {
                Date = date,
                DayNumber = date.Day.ToString(),
                DisplayText = displayBase,
                StatusIcon = success ? "✓" : "✗",
                StatusColor = success ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828")
            });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class CalendarDay
{
    public DateTime Date { get; set; }
    public string DisplayText { get; set; }
    public string DayNumber { get; set; }
    public string StatusIcon { get; set; }
    public Color StatusColor { get; set; }
}
