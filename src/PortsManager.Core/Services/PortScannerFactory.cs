#nullable enable
namespace PortsManager.Core.Services;

public sealed class PortScannerFactory
{
    public IPortScanner CreateScanner(CommandRunner commandRunner)
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsPortScanner(commandRunner);
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacPortScanner(commandRunner);
        }

        return new LinuxPortScanner(commandRunner);
    }
}
