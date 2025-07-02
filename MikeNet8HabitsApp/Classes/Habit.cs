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

    // Used by UI to decide whether to show count-related controls
    public bool IsCountable { get; set; } = false;
}