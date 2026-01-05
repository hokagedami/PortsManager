#nullable enable
using System.Diagnostics;

namespace PortsManager.Core.Services;

public sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);

public sealed class CommandRunner
{
    public async Task<CommandResult> RunAsync(string fileName, string arguments, TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException($"Failed to start '{fileName}'.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var waitTask = process.WaitForExitAsync();

        var completed = await Task.WhenAny(waitTask, Task.Delay(timeout));
        if (completed != waitTask)
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // Ignore kill failures when timing out.
            }
        }
        else
        {
            await waitTask;
        }

        var output = await outputTask;
        var error = await errorTask;

        return new CommandResult(process.ExitCode, output, error);
    }
}
