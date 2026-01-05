#nullable enable
namespace PortsManager.Core.Models;

public sealed class PortProcessInfo
{
    public required string Protocol { get; init; }
    public required int Port { get; init; }
    public required int Pid { get; init; }
    public required string ProcessName { get; init; }
    public required string Status { get; init; }
}
