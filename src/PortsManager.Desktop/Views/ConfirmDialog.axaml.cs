#nullable enable
using Avalonia.Controls;

namespace PortsManager.Desktop.Views;

public sealed partial class ConfirmDialog : Window
{
    public ConfirmDialog() : this("Confirm", "", "OK", "Cancel") { }

    public ConfirmDialog(string title, string message, string okText, string cancelText)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
        OkButton.Content = okText;
        CancelButton.Content = cancelText;
        OkButton.Click += (_, _) => Close(true);
        CancelButton.Click += (_, _) => Close(false);
    }

    public static Task<bool> Show(Window owner, string title, string message, string okText = "OK", string cancelText = "Cancel")
    {
        var dialog = new ConfirmDialog(title, message, okText, cancelText);
        return dialog.ShowDialog<bool>(owner);
    }
}
