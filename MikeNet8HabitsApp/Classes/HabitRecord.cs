using SQLite;
using System;
using System.ComponentModel;
using System.Globalization;

namespace MikeNet8HabitsApp.Classes;

[Table("HabitRecords")]
public class HabitRecord : INotifyPropertyChanged
{
    private DateTime _date;
    private string _dateString;

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public int HabitId { get; set; }
    
    [Ignore]
    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value.Date;
                DateString = _date.ToString("yyyy-MM-dd");
                OnPropertyChanged(nameof(Date));
            }
        }
    }
    
    [Column("Date")]
    public string DateString
    {
        get => _dateString;
        set
        {
            if (_dateString != value)
            {
                _dateString = value;
                if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    _date = parsedDate;
                }
                OnPropertyChanged(nameof(DateString));
            }
        }
    }
    
    public bool IsCompleted { get; set; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
