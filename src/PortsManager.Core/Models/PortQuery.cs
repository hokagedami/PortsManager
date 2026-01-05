#nullable enable
namespace PortsManager.Core.Models;

public sealed class PortQuery
{
    public int? Port { get; init; }
    public string? ProcessNameContains { get; init; }
}
