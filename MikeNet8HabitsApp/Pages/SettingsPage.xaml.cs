using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Maui.Storage;
using MikeNet8HabitsApp.Services;

namespace MikeNet8HabitsApp.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsService _settings;
    private readonly DatabaseService _db;

    public SettingsPage()
    {
        InitializeComponent();
        _settings = App.Current.Handler.MauiContext.Services.GetService<SettingsService>();
        _db = App.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
        ThresholdSlider.Value = _settings.ThresholdPercent;
        ThresholdLabel.Text = $"{_settings.ThresholdPercent}%";
    }

    private void OnThresholdChanged(object sender, ValueChangedEventArgs e)
    {
        int val = (int)e.NewValue;
        _settings.ThresholdPercent = val;
        ThresholdLabel.Text = $"{val}%";
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        var (json, suggestedName) = await _settings.BuildExportJsonAsync(_db);
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        var saveResult = await FileSaver.Default.SaveAsync(suggestedName, stream, CancellationToken.None);
        if (saveResult.IsSuccessful)
            await DisplayAlert("Export", $"Saved to {saveResult.FilePath}", "OK");
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        // Merge import – keeps existing data, skips duplicates
        var pickResult = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select habits export" });
        if (pickResult == null) return;
        await _settings.ImportAsync(_db, pickResult.FullPath);
        await DisplayAlert("Import", "Merge import finished", "OK");
    }

    private async void OnDeleteDbClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Delete all data", "Are you sure you want to delete ALL data? This cannot be undone.", "Delete", "Cancel");
        if (!confirm) return;
        await _db.ResetDatabaseAsync();
        await DisplayAlert("Delete", "Database reset completed.", "OK");
    }

    private async void OnOverwriteImportClicked(object sender, EventArgs e)
    {
        var pickResult = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select habits export" });
        if (pickResult == null) return;
        bool confirm = await DisplayAlert("Overwrite Import", "This will delete current data and import from the selected file. Continue?", "Import", "Cancel");
        if (!confirm) return;

        await _db.ResetDatabaseAsync();
        await _settings.ImportAsync(_db, pickResult.FullPath);
        await DisplayAlert("Import", "Overwrite import finished", "OK");
    }
}
