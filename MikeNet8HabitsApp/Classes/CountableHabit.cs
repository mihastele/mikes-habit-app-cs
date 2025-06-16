namespace MikeNet8HabitsApp.Classes;

public class CountableHabit : Habit
{
    public int TargetCount { get; set; } = 0;
    public int CurrentCount { get; set; } = 0;

    public CountableHabit()
    {
        // Ensure default color is set for new CountableHabit instances
        Color = Colors.LightGray;
    }

    public override bool IsCountable => true;
}