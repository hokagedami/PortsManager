#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using PortsManager.Desktop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PortsManager.Desktop.Views;

public sealed partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private bool _isLoading;

    public SettingsWindow()
    {
        InitializeComponent();
        _settings = AppSettings.Load();
        LoadSettings();
        RefreshIntervalInput.ValueChanged += OnIntervalChanged;
    }

    private void LoadSettings()
    {
        _isLoading = true;

        AutoRefreshCheckBox.IsChecked = _settings.AutoRefreshEnabled;
        RefreshIntervalInput.Value = (decimal)_settings.RefreshIntervalSeconds;
        ConfirmTerminateCheckBox.IsChecked = _settings.ConfirmBeforeTerminate;
        ShowSystemProcessesCheckBox.IsChecked = _settings.ShowSystemProcesses;
        StartMinimizedCheckBox.IsChecked = _settings.StartMinimized;

        var themeItem = ThemeComboBox.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Tag?.ToString() == _settings.Theme);
        if (themeItem is not null)
        {
            ThemeComboBox.SelectedItem = themeItem;
        }

        _isLoading = false;
    }

    private void OnSettingChanged(object? sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        SaveSettings();
    }

    private void OnIntervalChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isLoading) return;
        SaveSettings();
    }

    private void OnThemeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isLoading) return;
        SaveSettings();
        ApplyTheme();
    }

    private void SaveSettings()
    {
        _settings.AutoRefreshEnabled = AutoRefreshCheckBox.IsChecked ?? false;
        _settings.RefreshIntervalSeconds = (int)(RefreshIntervalInput.Value ?? 5m);
        _settings.ConfirmBeforeTerminate = ConfirmTerminateCheckBox.IsChecked ?? true;
        _settings.ShowSystemProcesses = ShowSystemProcessesCheckBox.IsChecked ?? true;
        _settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;

        if (ThemeComboBox.SelectedItem is ComboBoxItem selectedTheme)
        {
            _settings.Theme = selectedTheme.Tag?.ToString() ?? "Light";
        }

        _settings.Save();
    }

    private void ApplyTheme()
    {
        if (Application.Current is null) return;

        Application.Current.RequestedThemeVariant = _settings.Theme switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    public static async Task Open(Window owner)
    {
        var dialog = new SettingsWindow();
        await dialog.ShowDialog(owner);
    }
}
