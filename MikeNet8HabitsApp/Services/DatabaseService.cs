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
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
        _connection = new SQLiteAsyncConnection(_databasePath);
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
        
        // Settings table for future extension (key/value). Currently Preferences API is used instead.
        // await _connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS Settings (Key TEXT PRIMARY KEY, Value TEXT)");
        
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
    
    public async Task<HabitRecord> GetHabitRecordAsync(int habitId, DateTime date)
    {
        // Ensure we're only comparing the date part
        var dateOnly = date.Date;
    
        // Query using exact date match
        return await _connection.Table<HabitRecord>()
            .Where(r => r.HabitId == habitId && r.Date == dateOnly)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveHabitRecordAsync(HabitRecord record)
    {
        // Ensure we only store the date part
        record.Date = record.Date.Date;
    
        // Check if a record already exists for this habit and date
        var existingRecord = await GetHabitRecordAsync(record.HabitId, record.Date);
    
        if (existingRecord != null)
        {
            // Update existing record
            record.Id = existingRecord.Id;  // Ensure we're updating the correct record
            return await _connection.UpdateAsync(record);
        }
        else
        {
            // Insert new record
            return await _connection.InsertAsync(record);
        }
    }
    
    // public async Task<int> SaveHabitRecordAsync(HabitRecord record)
    // {
    //     // Ensure we only store the date part
    //     record.Date = record.Date.Date;
    //     return await _connection.InsertOrReplaceAsync(record);
    // }

    // public async Task SaveHabitRecordAsync(HabitRecord record)
    // {
    //     await _connection.InsertOrReplaceAsync(record);
    // }

    public async Task DeleteHabitAsync(int id)
    {
        // Remove all habit records associated with this habit first
        await _connection.ExecuteAsync("DELETE FROM HabitRecord WHERE HabitId = ?", id);

        // Now remove the habit itself (or countable habit)
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

    // public async Task<HabitRecord> GetHabitRecordAsync(int habitId, DateTime date)
    // {
    //     // Ensure we're only comparing the date part
    //     var dateOnly = date.Date;
    //     var nextDay = dateOnly.AddDays(1);
    //     
    //     // Query using date range to match the day
    //     return await _connection.Table<HabitRecord>()
    //         .Where(r => r.HabitId == habitId && 
    //                   r.Date >= dateOnly && 
    //                   r.Date < nextDay)
    //         .FirstOrDefaultAsync();
    // }
    //
    public async Task<List<HabitRecord>> GetHabitRecordsForHabitId(int habitId)
    {
        // Query using the string-based DateString property
        return await _connection.Table<HabitRecord>()
            .Where(r => r.HabitId == habitId).ToListAsync();
    }
    
    public async Task<HabitRecord> DebugHabitRecordAsync()
    {
        return await _connection.Table<HabitRecord>().FirstOrDefaultAsync();
    }

    public async Task<List<HabitRecord>> GetAllHabitRecordsForHabitAsync(int habitId)
    {
        return await _connection.Table<HabitRecord>().Where(r => r.HabitId == habitId).ToListAsync();
    }

    public async Task<List<HabitRecord>> GetAllHabitRecordsAsync()
    {
        return await _connection.Table<HabitRecord>().ToListAsync();
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

    /// <summary>
    /// Permanently deletes all application data by dropping tables and recreating them.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await _connection.DropTableAsync<Habit>();
        await _connection.DropTableAsync<CountableHabit>();
        await _connection.DropTableAsync<HabitRecord>();
        await InitializeAsync();
    }
}
