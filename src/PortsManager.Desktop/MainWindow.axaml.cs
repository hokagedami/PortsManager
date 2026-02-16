using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using PortsManager.Core.Services;
using PortsManager.Desktop.Models;
using PortsManager.Desktop.ViewModels;
using PortsManager.Desktop.Views;
using System.Threading.Tasks;

namespace PortsManager.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly PortListViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new PortListViewModel();
        DataContext = _viewModel;
        Opened += OnWindowOpened;
        Closed += (_, _) => _viewModel.Dispose();
        ApplyThemeFromSettings();
    }

    private static void ApplyThemeFromSettings()
    {
        if (Application.Current is null) return;
        var settings = AppSettings.Load();
        Application.Current.RequestedThemeVariant = settings.Theme switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            await AlertDialog.Show(this, "Error", ex.Message);
        }
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            await AlertDialog.Show(this, "Error", ex.Message);
        }
    }

    private async void OnTerminateClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedPort is null)
        {
            await AlertDialog.Show(this, "No Selection", "Select a port to terminate.");
            return;
        }

        var confirm = await ConfirmDialog.Show(this, "Confirm Termination",
            $"Terminate {_viewModel.SelectedPort.ProcessName} (PID {_viewModel.SelectedPort.Pid})?");
        if (!confirm)
        {
            return;
        }

        var result = await _viewModel.GracefulTerminateSelectedAsync();
        if (result.Kind == TerminationResultKind.Success)
        {
            await _viewModel.RefreshAsync();
            return;
        }

        if (result.Kind == TerminationResultKind.StillRunning)
        {
            var forceConfirm = await ConfirmDialog.Show(this, "Force Termination",
                "Process is still running. Force terminate?");
            if (!forceConfirm)
            {
                return;
            }

            var forceResult = await _viewModel.ForceTerminateSelectedAsync();
            await AlertDialog.Show(this, "Termination Result", forceResult.Message ?? forceResult.Kind.ToString());
            await _viewModel.RefreshAsync();
            return;
        }

        await AlertDialog.Show(this, "Termination Result", result.Message ?? result.Kind.ToString());
    }

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        var details = _viewModel.GetSelectedDetails();
        if (string.IsNullOrWhiteSpace(details))
        {
            await AlertDialog.Show(this, "No Selection", "Select a port to copy details.");
            return;
        }

        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard is null)
        {
            return;
        }

        await topLevel.Clipboard.SetTextAsync(details);
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        await SettingsWindow.Open(this);
        var settings = AppSettings.Load();
        _viewModel.ApplySettings(settings);
    }

    private void OnClearSearchClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.SearchText = string.Empty;
    }

    private async void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        await ShowDetailsDialog();
    }

    private async void OnViewDetailsClick(object? sender, RoutedEventArgs e)
    {
        await ShowDetailsDialog();
    }

    private async Task ShowDetailsDialog()
    {
        if (_viewModel.SelectedPort is null) return;
        await DetailsWindow.Open(this, _viewModel.SelectedPort);
    }
}
