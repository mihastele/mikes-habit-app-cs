using SQLite;

namespace MikeNet8HabitsApp.Classes;

[Table("CountableHabits")]  // Explicit table name to avoid conflicts
public class CountableHabit : Habit
{
    public int TargetCount { get; set; } = 0;
    public int CurrentCount { get; set; } = 0;

    public CountableHabit() : base()
    {
        // Ensure default color is set for new CountableHabit instances
        Color = Colors.LightGray;
    }

    [Ignore]  // This property is already defined in the base class
    public override bool IsCountable => true;
    
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