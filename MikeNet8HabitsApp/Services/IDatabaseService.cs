using System.Collections.Generic;
using System.Threading.Tasks;
using MikeNet8HabitsApp.Classes;

namespace MikeNet8HabitsApp.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Habit>> GetAllHabitsAsync();
    Task<int> SaveHabitAsync(Habit habit);
    Task ResetDatabaseAsync();
    Task<HabitRecord> GetHabitRecordAsync(int habitId, DateTime date);
    Task<List<HabitRecord>> GetAllHabitRecordsForHabitAsync(int habitId);
}
