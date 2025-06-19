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
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
        _connection = new SQLiteAsyncConnection(databasePath);
    }

    public async Task InitializeAsync()
    {
        
        // Drop existing tables to handle schema changes
        // await _connection.DropTableAsync<Habit>();
        // await _connection.DropTableAsync<CountableHabit>();
        // await _connection.DropTableAsync<HabitRecord>();
        
        // Create new tables with updated schema if they do not exist
        await _connection.CreateTableAsync<Habit>();
        await _connection.CreateTableAsync<CountableHabit>();
        await _connection.CreateTableAsync<HabitRecord>();
        
        // For debugging: Verify tables were ensured
        var tableInfo = await _connection.GetTableInfoAsync("Habit");
        System.Diagnostics.Debug.WriteLine($"Habit table ensured with {tableInfo.Count} columns");
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
            return await UpdateOrInsertCountableHabitAsync(countable);
        }
        return await UpdateOrInsertHabitAsync(habit);
    }

    public async Task SaveHabitRecordAsync(HabitRecord record)
    {
        await _connection.InsertOrReplaceAsync(record);
    }

    public async Task DeleteHabitAsync(int id)
    {
        var habit = await _connection.FindAsync<Habit>(id);
        if (habit != null)
        {
            await _connection.DeleteAsync(habit);
        }
        else
        {
            var countableHabit = await _connection.FindAsync<CountableHabit>(id);
            if (countableHabit != null)
            {
                await _connection.DeleteAsync(countableHabit);
            }
        }
    }

    public async Task<HabitRecord> GetHabitRecordAsync(int habitId, DateTime date)
    {
        // Format the date as yyyy-MM-dd string for comparison
        var dateString = date.ToString("yyyy-MM-dd");
        
        // Query using the string-based DateString property
        return await _connection.Table<HabitRecord>()
            .Where(r => r.HabitId == habitId && r.DateString == dateString)
            .FirstOrDefaultAsync();
    }
    
    public async Task<HabitRecord> DebugHabitRecordAsync()
    {
        return await _connection.Table<HabitRecord>().FirstOrDefaultAsync();
    }

    public async Task<List<HabitRecord>> GetAllHabitRecordsForHabitAsync(int habitId)
    {
        return await _connection.Table<HabitRecord>().Where(r => r.HabitId == habitId).ToListAsync();
    }

    private async Task<int> UpdateOrInsertCountableHabitAsync(CountableHabit countable)
    {
        if (countable.Id != 0)
            return await _connection.UpdateAsync(countable);
        return await _connection.InsertAsync(countable);
    }

    private async Task<int> UpdateOrInsertHabitAsync(Habit habit)
    {
        if (habit.Id != 0)
            return await _connection.UpdateAsync(habit);
        return await _connection.InsertAsync(habit);
    }
}
