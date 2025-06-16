using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using MikeNet8HabitsApp.Classes;
using Microsoft.Maui.Storage;

namespace MikeNet8HabitsApp.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _connection;

    public DatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
        _connection = new SQLiteAsyncConnection(dbPath);
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _connection.CreateTableAsync<Habit>();
        await _connection.CreateTableAsync<CountableHabit>();
    }

    // Returns all habits (including countable ones) ordered by Id.
    public async Task<List<Habit>> GetAllHabitsAsync()
    {
        var habits = await _connection.Table<Habit>().ToListAsync();
        var countables = await _connection.Table<CountableHabit>().ToListAsync();

        // Merge lists preserving derived type info
        var result = new List<Habit>();
        result.AddRange(habits);
        result.AddRange(countables);
        result.Sort((a, b) => a.Id.CompareTo(b.Id));
        return result;
    }

    public async Task<int> SaveHabitAsync(Habit habit)
    {
        if (habit is CountableHabit countable)
        {
            if (countable.Id != 0)
                return await _connection.UpdateAsync(countable);
            return await _connection.InsertAsync(countable);
        }

        if (habit.Id != 0)
            return await _connection.UpdateAsync(habit);
        return await _connection.InsertAsync(habit);
    }
}
