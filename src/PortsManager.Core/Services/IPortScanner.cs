#nullable enable
using PortsManager.Core.Models;

namespace PortsManager.Core.Services;

public interface IPortScanner
{
    Task<IReadOnlyList<PortProcessInfo>> ScanAsync(CancellationToken cancellationToken);
}
