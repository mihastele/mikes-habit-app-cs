namespace MikeNet8HabitsApp.Classes;

public class Habit
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public Color Color { get; set; } = Colors.LightGray; // Default color
    public int Streak { get; set; }
}