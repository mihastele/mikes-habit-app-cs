using SQLite;
using Microsoft.Maui.Graphics;

namespace MikeNet8HabitsApp.Classes;

[Table("Habit")]  // Map to the same "Habit" table so IDs are unique across all habit types
public class CountableHabit : Habit
{
    public int TargetCount { get; set; } = 0;
    
    [Ignore] // Do not persist daily progress in the Habit table – it belongs in HabitRecord
    public int CurrentCount { get; set; } = 0;

    public CountableHabit() : base()
    {
        // Mark this as a countable habit and set default color
        IsCountable = true;
        Color = Colors.LightGray;
    }
    
    // Add a method to update counts safely
    public void UpdateCount(int change)
    {
        var newCount = CurrentCount + change;
        if (newCount >= 0 && newCount <= TargetCount)
        {
            CurrentCount = newCount;
            IsCompleted = (CurrentCount >= TargetCount);
        }
    }
}