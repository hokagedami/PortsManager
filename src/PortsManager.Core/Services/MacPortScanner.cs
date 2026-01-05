#nullable enable
using PortsManager.Core.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace PortsManager.Core.Services;

public sealed class MacPortScanner : IPortScanner
{
    private readonly CommandRunner _commandRunner;
    private static readonly Regex PortRegex = new(@":(\d+)", RegexOptions.Compiled);

    public MacPortScanner(CommandRunner commandRunner)
    {
        _commandRunner = commandRunner;
    }

    public async Task<IReadOnlyList<PortProcessInfo>> ScanAsync(CancellationToken cancellationToken)
    {
        var tcp = await _commandRunner.RunAsync("lsof", "-nP -iTCP -sTCP:LISTEN", TimeSpan.FromSeconds(3));
        var udp = await _commandRunner.RunAsync("lsof", "-nP -iUDP", TimeSpan.FromSeconds(3));

        var results = new List<PortProcessInfo>();
        results.AddRange(ParseLsofOutput(tcp.StandardOutput, "TCP"));
        results.AddRange(ParseLsofOutput(udp.StandardOutput, "UDP"));

        return results;
    }

    private static IEnumerable<PortProcessInfo> ParseLsofOutput(string output, string protocol)
    {
        var results = new List<PortProcessInfo>();
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("COMMAND", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(tokens[1], out var pid))
            {
                continue;
            }

            var match = PortRegex.Matches(trimmed).Cast<Match>().LastOrDefault();
            if (match is null || !int.TryParse(match.Groups[1].Value, out var port))
            {
                continue;
            }

            results.Add(new PortProcessInfo
            {
                Protocol = protocol.ToUpperInvariant(),
                Port = port,
                Pid = pid,
                ProcessName = tokens[0],
                Status = "Listening"
            });
        }

        return results;
    }
}
