#nullable enable
using Avalonia.Controls;

namespace PortsManager.Desktop.Views;

public sealed partial class AlertDialog : Window
{
    public AlertDialog() : this("Alert", "") { }

    public AlertDialog(string title, string message)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
        OkButton.Click += (_, _) => Close();
    }

    public static Task Show(Window owner, string title, string message)
    {
        var dialog = new AlertDialog(title, message);
        return dialog.ShowDialog(owner);
    }
}
