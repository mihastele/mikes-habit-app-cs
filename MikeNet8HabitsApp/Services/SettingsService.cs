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
    /// Export all habits, records and settings to a JSON file inside <see cref="FileSystem.AppDataDirectory"/>.
    /// File name pattern: habits_export_yyyyMMddHHmmss.json
    /// Returns the full file path.
    /// </summary>
    public async Task<string> ExportAsync(DatabaseService db)
    {
        var exportObj = new
        {
            Settings = new { ThresholdPercent },
            Habits = await db.GetAllHabitsAsync(),
            // For simplicity load all records once – adjust if dataset becomes large
            HabitRecords = await db.GetAllHabitRecordsAsync()
        };

        var json = JsonSerializer.Serialize(exportObj, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"habits_export_{DateTime.Now:yyyyMMddHHmmss}.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }

    /// <summary>
    /// Import previously exported JSON file. Existing DB rows are left intact – duplicates are skipped.
    /// </summary>
    public async Task ImportAsync(DatabaseService db, string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = await File.ReadAllTextAsync(filePath);
        var doc = JsonDocument.Parse(json).RootElement;

        if (doc.TryGetProperty("Settings", out var s) && s.TryGetProperty("ThresholdPercent", out var tp))
            ThresholdPercent = tp.GetInt32();

        if (doc.TryGetProperty("Habits", out var h))
        {
            foreach (var habitElem in h.EnumerateArray())
            {
                var habit = JsonSerializer.Deserialize<Habit>(habitElem); // base class covers both derived types thanks to SQLite attrs
                await db.SaveHabitAsync(habit);
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
}
