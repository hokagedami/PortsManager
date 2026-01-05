using PortsManager.Core.Models;
using PortsManager.Core.Utilities;
using System.Text.RegularExpressions;

namespace PortsManager.Core.Services;

public sealed class LinuxPortScanner(CommandRunner commandRunner) : IPortScanner
{
    private static readonly Regex PidRegex = new(@"pid=(\d+)", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new("\"([^\"]+)\"", RegexOptions.Compiled);

    public async Task<IReadOnlyList<PortProcessInfo>> ScanAsync(CancellationToken cancellationToken)
    {
        var output = await commandRunner.RunAsync("ss", "-lntup", TimeSpan.FromSeconds(3));
        return ParseSsOutput(output.StandardOutput);
    }

    private static List<PortProcessInfo> ParseSsOutput(string output)
    {
        var results = new List<PortProcessInfo>();
        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Netid", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 5)
            {
                continue;
            }

            var netid = tokens[0];
            var protocol = netid.StartsWith("tcp", StringComparison.OrdinalIgnoreCase) ? "TCP" : "UDP";
            var local = tokens[4];

            if (!EndpointParser.TryParsePort(local, out var port))
            {
                continue;
            }

            var pidMatch = PidRegex.Match(trimmed);
            var pid = pidMatch.Success && int.TryParse(pidMatch.Groups[1].Value, out var parsedPid) ? parsedPid : 0;

            var nameMatch = NameRegex.Match(trimmed);
            var processName = nameMatch.Success ? nameMatch.Groups[1].Value : "Unknown";

            results.Add(new PortProcessInfo
            {
                Protocol = protocol,
                Port = port,
                Pid = pid,
                ProcessName = processName,
                Status = "Listening"
            });
        }

        return results;
    }
}
