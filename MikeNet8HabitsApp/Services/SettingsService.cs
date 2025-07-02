using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using MikeNet8HabitsApp.Services;
using MikeNet8HabitsApp.Classes;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace MikeNet8HabitsApp.Services;

/// <summary>
/// Simple wrapper around <see cref="Preferences"/> to persist user-level settings plus helper import/export methods.
/// </summary>
public class SettingsService
{
    private const string ThresholdKey = "threshold_percent";
    private const int DefaultThreshold = 80;

    public int ThresholdPercent
    {
        get => Preferences.Get(ThresholdKey, DefaultThreshold);
        set => Preferences.Set(ThresholdKey, value);
    }

    /// <summary>
    /// Creates JSON containing all habits, records and settings. Used by UI to save to chosen location.
    /// Returns the JSON string and suggested filename.
    /// </summary>
    public async Task<(string json, string fileName)> BuildExportJsonAsync(DatabaseService db)
    {
        var exportObj = new
        {
            Settings = new { ThresholdPercent },
            Habits = await db.GetAllHabitsAsync(),
            // For simplicity load all records once – adjust if dataset becomes large
            HabitRecords = await db.GetAllHabitRecordsAsync()
        };

        var json = JsonSerializer.Serialize(exportObj, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"habits_export_{DateTime.Now:yyyyMMddHHmmss}.json";
        return (json, fileName);
    }

    /// <summary>
    /// Import previously exported JSON string. Existing DB rows are left intact – duplicates are skipped.
    /// </summary>
    public async Task ImportFromJsonAsync(DatabaseService db, string json)
    {
        var doc = JsonDocument.Parse(json).RootElement;

        if (doc.TryGetProperty("Settings", out var s) && s.TryGetProperty("ThresholdPercent", out var tp))
            ThresholdPercent = tp.GetInt32();

        if (doc.TryGetProperty("Habits", out var h))
        {
            foreach (var habitElem in h.EnumerateArray())
            {
                // Determine habit type based on serialized properties
                Habit habit;
                if (habitElem.TryGetProperty("IsCountable", out var countableProp) && countableProp.GetBoolean())
                {
                    habit = JsonSerializer.Deserialize<CountableHabit>(habitElem.GetRawText());
                }
                else
                {
                    habit = JsonSerializer.Deserialize<Habit>(habitElem.GetRawText());
                }
                await db.SaveHabitAsync(habit, true); // Import mode
            }
        }
        if (doc.TryGetProperty("HabitRecords", out var r))
        {
            foreach (var recElem in r.EnumerateArray())
            {
                var rec = JsonSerializer.Deserialize<HabitRecord>(recElem);
                await db.SaveHabitRecordAsync(rec);
            }
        }
    }

    /// <summary>
    /// Convenience wrapper to import by file path.
    /// </summary>
    public async Task ImportAsync(DatabaseService db, string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = await File.ReadAllTextAsync(filePath);
        await ImportFromJsonAsync(db, json);
    }
}
