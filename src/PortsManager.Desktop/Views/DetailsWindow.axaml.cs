#nullable enable
using Avalonia.Controls;
using Avalonia.Interactivity;
using PortsManager.Desktop.ViewModels;
using System.Threading.Tasks;

namespace PortsManager.Desktop.Views;

public sealed partial class DetailsWindow : Window
{
    public DetailsWindow()
    {
        InitializeComponent();
    }

    public void SetDetails(PortProcessItem item)
    {
        ProcessNameText.Text = item.ProcessName;
        PidText.Text = item.Pid.ToString();
        ProtocolText.Text = item.Protocol;
        PortText.Text = item.Port.ToString();
        StatusText.Text = item.Status;
        Title = $"Port {item.Port} - {item.ProcessName}";
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public static async Task Open(Window owner, PortProcessItem item)
    {
        var dialog = new DetailsWindow();
        dialog.SetDetails(item);
        await dialog.ShowDialog(owner);
    }
}
