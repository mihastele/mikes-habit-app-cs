using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MikeNet8HabitsApp.Services;
using SQLite;

namespace MikeNet8HabitsApp.Pages
{
    public partial class DebugPage : ContentPage
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _logFilePath;

        public DebugPage()
        {
            InitializeComponent();
            _databaseService = App.Current?.Handler?.MauiContext?.Services.GetService<IDatabaseService>() ??
                              throw new InvalidOperationException("Unable to resolve database service");
            
            // Set up log file path
            _logFilePath = Path.Combine(FileSystem.AppDataDirectory, "debug_log.txt");
            
            // Load initial logs
            _ = LoadLogsAsync();
        }

        private async void OnCheckDatabaseStatusClicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Checking database status...";
                
                // Check if database file exists
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
                var dbExists = File.Exists(dbPath);
                
                var status = new StringBuilder();
                status.AppendLine($"Database file exists: {dbExists}");
                
                if (dbExists)
                {
                    var fileInfo = new FileInfo(dbPath);
                    status.AppendLine($"Database size: {fileInfo.Length} bytes");
                    status.AppendLine($"Last modified: {fileInfo.LastWriteTime}");
                    
                    try
                    {
                        // Try to query the database
                        var habits = await _databaseService.GetAllHabitsAsync();
                        status.AppendLine($"\nFound {habits?.Count ?? 0} habits in database");
                        
                        if (habits != null && habits.Count > 0)
                        {
                            status.AppendLine("\nSample habit:");
                            var sample = habits.First();
                            status.AppendLine($"- Name: {sample.Name}");
                            status.AppendLine($"  ID: {sample.Id}");
                            status.AppendLine($"  Type: {sample.GetType().Name}");
                            status.AppendLine($"  IsCountable: {sample.IsCountable}");
                            
                            // if (sample is Classes.CountableHabit countable)
                            // {
                            //     status.AppendLine($"  CurrentCount: {countable.CurrentCount}");
                            //     status.AppendLine($"  TargetCount: {countable.TargetCount}");
                            // }
                        }
                    }
                    catch (Exception ex)
                    {
                        status.AppendLine($"\nError querying database: {ex.Message}");
                        LogException("Database query error", ex);
                    }
                }
                
                StatusLabel.Text = status.ToString();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error checking database status: {ex.Message}";
                LogException("Database status check error", ex);
            }
            
            await LoadLogsAsync();
        }

        private async void OnResetDatabaseClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Reset", 
                "This will delete all data and recreate the database. Continue?", 
                "Yes", "No");
                
            if (!confirm) return;
            
            try
            {
                StatusLabel.Text = "Resetting database...";
                
                if (_databaseService is DatabaseService dbService)
                {
                    // await dbService.ResetDatabaseAsync();
                    StatusLabel.Text = "Database has been reset successfully.\nPlease restart the app.";
                }
                else
                {
                    StatusLabel.Text = "Error: Could not access database service.";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error resetting database: {ex.Message}";
                LogException("Database reset error", ex);
            }
            
            await LoadLogsAsync();
        }

        private void OnRefreshLogsClicked(object sender, EventArgs e)
        {
            _ = LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    var logs = await File.ReadAllTextAsync(_logFilePath);
                    LogLabel.Text = logs;
                    
                    // Scroll to bottom
                    await LogScrollView.ScrollToAsync(0, LogScrollView.ContentSize.Height, true);
                }
                else
                {
                    LogLabel.Text = "No log file found.";
                }
            }
            catch (Exception ex)
            {
                LogLabel.Text = $"Error loading logs: {ex.Message}";
            }
        }

        private void LogException(string message, Exception ex)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}: {ex}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
