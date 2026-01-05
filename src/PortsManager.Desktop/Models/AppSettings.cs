#nullable enable
using System.Text.Json;

namespace PortsManager.Desktop.Models;

public sealed class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PortsManager",
        "settings.json");

    public bool AutoRefreshEnabled { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 5;
    public bool StartMinimized { get; set; }
    public bool ConfirmBeforeTerminate { get; set; } = true;
    public bool ShowSystemProcesses { get; set; } = true;
    public string Theme { get; set; } = "System";

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // Ignore errors, return defaults
        }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
