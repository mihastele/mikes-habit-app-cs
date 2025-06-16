namespace MikeNet8HabitsApp.Classes;

public class CountableHabit : Habit
{
    public int TargetCount { get; set; } = 0;
    public int CurrentCount { get; set; } = 0;

    public override bool IsCountable => true;
}