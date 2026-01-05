#nullable enable
using PortsManager.Core.Models;
using PortsManager.Core.Utilities;

namespace PortsManager.Core.Services;

public sealed class WindowsPortScanner(CommandRunner commandRunner) : IPortScanner
{
    public async Task<IReadOnlyList<PortProcessInfo>> ScanAsync(CancellationToken cancellationToken)
    {
        // Use full path to netstat to avoid PATH issues
        var output = await commandRunner.RunAsync(
            @"C:\Windows\System32\netstat.exe",
            "-ano",
            TimeSpan.FromSeconds(30));

        return ParseNetstatOutput(output.StandardOutput);
    }

    private static List<PortProcessInfo> ParseNetstatOutput(string output)
    {
        var results = new List<PortProcessInfo>();

        if (string.IsNullOrWhiteSpace(output))
        {
            return results;
        }

        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            string protocol;
            if (trimmed.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
            {
                protocol = "TCP";
            }
            else if (trimmed.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
            {
                protocol = "UDP";
            }
            else
            {
                continue;
            }

            var tokens = trimmed.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 4)
            {
                continue;
            }

            // For TCP: Proto LocalAddr ForeignAddr State PID
            // For UDP: Proto LocalAddr ForeignAddr PID
            var localAddress = tokens[1];
            if (!TryParsePort(localAddress, out var port))
            {
                continue;
            }

            var pidToken = tokens[^1];
            if (!int.TryParse(pidToken, out var pid))
            {
                continue;
            }

            string status;
            if (protocol == "TCP")
            {
                if (tokens.Length < 5)
                {
                    continue;
                }
                status = tokens[3];
                // Only include listening TCP connections
                if (!status.Equals("LISTENING", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
            else
            {
                // UDP doesn't have state - all bound UDP ports are "listening"
                status = "Listening";
            }

            results.Add(new PortProcessInfo
            {
                Protocol = protocol,
                Port = port,
                Pid = pid,
                ProcessName = "Unknown",
                Status = status
            });
        }

        return results;
    }

    private static bool TryParsePort(string endpoint, out int port)
    {
        port = 0;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return false;
        }

        // Handle IPv6 format like [::]:port or [::1]:port
        // Handle IPv4 format like 0.0.0.0:port or 127.0.0.1:port
        var lastColon = endpoint.LastIndexOf(':');
        if (lastColon < 0 || lastColon >= endpoint.Length - 1)
        {
            return false;
        }

        var portPart = endpoint[(lastColon + 1)..];
        return int.TryParse(portPart, out port);
    }
}
