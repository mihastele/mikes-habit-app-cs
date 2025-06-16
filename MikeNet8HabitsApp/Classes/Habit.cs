using SQLite;
using Microsoft.Maui.Graphics;

namespace MikeNet8HabitsApp.Classes;

public class Habit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    
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
    public virtual bool IsCountable => false;
}