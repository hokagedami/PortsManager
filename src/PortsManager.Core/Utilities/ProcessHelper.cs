#nullable enable
using System.Diagnostics;

namespace PortsManager.Core.Utilities;

public static class ProcessHelper
{
    public static string? TryGetProcessName(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsProcessAlive(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> WaitForExitAsync(int pid, TimeSpan timeout)
    {
        var start = Stopwatch.StartNew();
        while (start.Elapsed < timeout)
        {
            if (!IsProcessAlive(pid))
            {
                return true;
            }

            await Task.Delay(200);
        }

        return !IsProcessAlive(pid);
    }
}
