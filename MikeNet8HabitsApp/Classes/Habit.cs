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
    public Color Color { get; set; } = Colors.LightGray; // Default color
    public int Streak { get; set; }

    // Used by UI to decide whether to show count-related controls
    public virtual bool IsCountable => false;
}