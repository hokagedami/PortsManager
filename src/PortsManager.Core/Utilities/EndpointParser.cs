#nullable enable
namespace PortsManager.Core.Utilities;

public static class EndpointParser
{
    public static bool TryParsePort(string endpoint, out int port)
    {
        port = 0;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return false;
        }

        var lastColon = endpoint.LastIndexOf(':');
        if (lastColon <= 0 || lastColon >= endpoint.Length - 1)
        {
            return false;
        }

        var portPart = endpoint[(lastColon + 1)..].TrimEnd(']');
        return int.TryParse(portPart, out port);
    }
}
