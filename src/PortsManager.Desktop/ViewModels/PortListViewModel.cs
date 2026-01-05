#nullable enable
using Avalonia.Threading;
using PortsManager.Core.Models;
using PortsManager.Core.Services;
using PortsManager.Desktop.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortsManager.Desktop.ViewModels;

public sealed class PortProcessItem
{
    public required string Protocol { get; init; }
    public required int Port { get; init; }
    public required int Pid { get; init; }
    public required string ProcessName { get; init; }
    public required string Status { get; init; }
}

public sealed class PortListViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly PortScanService _scanService;
    private readonly ProcessTerminator _terminator;
    private readonly DispatcherTimer _timer;
    private PortProcessItem? _selectedPort;
    private bool _autoRefreshEnabled;
    private int _autoRefreshSeconds = 5;
    private string _selectedDetails = string.Empty;
    private string _statusMessage = "Ready";
    private string _searchText = string.Empty;
    private bool _disposed;
    private List<PortProcessItem> _allPorts = [];

    public ObservableCollection<PortProcessItem> Ports { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public PortProcessItem? SelectedPort
    {
        get => _selectedPort;
        set
        {
            if (SetField(ref _selectedPort, value))
            {
                SelectedDetails = BuildDetails(value);
            }
        }
    }

    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            if (SetField(ref _autoRefreshEnabled, value))
            {
                UpdateTimer();
            }
        }
    }

    public int AutoRefreshSeconds
    {
        get => _autoRefreshSeconds;
        set
        {
            var normalized = Math.Clamp(value, 2, 60);
            if (SetField(ref _autoRefreshSeconds, normalized))
            {
                UpdateTimer();
            }
        }
    }

    public string SelectedDetails
    {
        get => _selectedDetails;
        private set => SetField(ref _selectedDetails, value);
    }

    public PortListViewModel()
    {
        var commandRunner = new CommandRunner();
        var scanner = new PortScannerFactory().CreateScanner(commandRunner);
        _scanService = new PortScanService(scanner);
        _terminator = new ProcessTerminator(commandRunner);

        var settings = AppSettings.Load();
        _autoRefreshEnabled = settings.AutoRefreshEnabled;
        _autoRefreshSeconds = settings.RefreshIntervalSeconds;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_autoRefreshSeconds) };
        _timer.Tick += async (_, _) => await RefreshAsync();

        if (_autoRefreshEnabled)
        {
            _timer.Start();
        }
    }

    public async Task RefreshAsync()
    {
        StatusMessage = "Scanning...";

        try
        {
            var results = await _scanService.ListAsync(new PortQuery(), CancellationToken.None);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allPorts = results.Select(item => new PortProcessItem
                {
                    Protocol = item.Protocol,
                    Port = item.Port,
                    Pid = item.Pid,
                    ProcessName = item.ProcessName,
                    Status = item.Status
                }).ToList();

                ApplyFilter();
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            throw;
        }
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(_searchText)
            ? _allPorts
            : _allPorts.Where(p =>
                p.Port.ToString().Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                p.ProcessName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Pid.ToString().Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Protocol.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        Ports.Clear();
        foreach (var item in filtered)
        {
            Ports.Add(item);
        }

        StatusMessage = string.IsNullOrWhiteSpace(_searchText)
            ? $"Found {_allPorts.Count} listening ports"
            : $"Showing {filtered.Count} of {_allPorts.Count} ports";
    }

    public async Task<TerminationResult> GracefulTerminateSelectedAsync()
    {
        if (SelectedPort is null)
        {
            return TerminationResult.Failed("No process selected.");
        }

        return await _terminator.GracefulTerminateAsync(SelectedPort.Pid, TimeSpan.FromSeconds(5));
    }

    public async Task<TerminationResult> ForceTerminateSelectedAsync()
    {
        if (SelectedPort is null)
        {
            return TerminationResult.Failed("No process selected.");
        }

        return await _terminator.ForceTerminateAsync(SelectedPort.Pid, TimeSpan.FromSeconds(5));
    }

    public string GetSelectedDetails()
    {
        return SelectedDetails;
    }

    public void ApplySettings(AppSettings settings)
    {
        AutoRefreshEnabled = settings.AutoRefreshEnabled;
        AutoRefreshSeconds = settings.RefreshIntervalSeconds;
    }

    private void UpdateTimer()
    {
        _timer.Stop();
        if (!_autoRefreshEnabled)
        {
            return;
        }

        _timer.Interval = TimeSpan.FromSeconds(_autoRefreshSeconds);
        _timer.Start();
    }

    private static string BuildDetails(PortProcessItem? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        return $"Process: {item.ProcessName}\nPID: {item.Pid}\nProtocol: {item.Protocol}\nPort: {item.Port}\nStatus: {item.Status}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer.Stop();
        _disposed = true;
    }
}
