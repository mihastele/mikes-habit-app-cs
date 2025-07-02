using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using Microsoft.Maui.Graphics;

namespace MikeNet8HabitsApp.Classes;

public class Habit : INotifyPropertyChanged
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    
    private bool _isCompleted;
    
    [Ignore]
    public bool IsCompleted 
    { 
        get => _isCompleted; 
        set 
        {
            if (_isCompleted != value)
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
    private string _colorHex = Colors.LightGray.ToHex();
    public string ColorHex 
    { 
        get => _colorHex;
        set => _colorHex = value ?? Colors.LightGray.ToHex();
    }
    
    [Ignore]
    public Color Color
    {
        get => Color.FromArgb(ColorHex);
        set => ColorHex = value.ToHex();
    }
    
    public int Streak { get; set; }

    // Countable habit properties
    public bool IsCountable { get; set; } = false;
    public int TargetCount { get; set; } = 0;
    
    [Ignore] // Not persisted - derived from HabitRecord
    public int CurrentCount { get; set; } = 0;

    // Helper method for countable habits
    public void UpdateCount(int change)
    {
        if (!IsCountable) return;
        
        var newCount = CurrentCount + change;
        if (newCount >= 0 && newCount <= TargetCount)
        {
            CurrentCount = newCount;
            IsCompleted = (CurrentCount >= TargetCount);
            OnPropertyChanged(nameof(CurrentCount));
            OnPropertyChanged(nameof(IsCompleted));
        }
    }
}