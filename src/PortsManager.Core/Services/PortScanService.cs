#nullable enable
using PortsManager.Core.Models;
using PortsManager.Core.Utilities;

namespace PortsManager.Core.Services;

public sealed class PortScanService
{
    private readonly IPortScanner _scanner;

    public PortScanService(IPortScanner scanner)
    {
        _scanner = scanner;
    }

    public async Task<IReadOnlyList<PortProcessInfo>> ListAsync(PortQuery query, CancellationToken cancellationToken)
    {
        var results = await _scanner.ScanAsync(cancellationToken);
        var enriched = new List<PortProcessInfo>(results.Count);

        foreach (var entry in results)
        {
            var processName = entry.ProcessName;
            if (string.IsNullOrWhiteSpace(processName) || processName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                processName = ProcessHelper.TryGetProcessName(entry.Pid) ?? "Unknown";
            }

            var status = string.IsNullOrWhiteSpace(entry.Status) ? "Listening" : entry.Status;
            var normalized = new PortProcessInfo
            {
                Protocol = entry.Protocol,
                Port = entry.Port,
                Pid = entry.Pid,
                ProcessName = processName,
                Status = status
            };

            enriched.Add(normalized);
        }

        IEnumerable<PortProcessInfo> filtered = enriched;
        if (query.Port is not null)
        {
            filtered = filtered.Where(p => p.Port == query.Port.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessNameContains))
        {
            filtered = filtered.Where(p => p.ProcessName.Contains(query.ProcessNameContains, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.OrderBy(p => p.Port).ThenBy(p => p.Protocol).ToList();
    }
}
