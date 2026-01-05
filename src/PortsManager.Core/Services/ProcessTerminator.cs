#nullable enable
using PortsManager.Core.Utilities;

namespace PortsManager.Core.Services;

public sealed class ProcessTerminator : IProcessTerminator
{
    private readonly CommandRunner _commandRunner;

    public ProcessTerminator(CommandRunner commandRunner)
    {
        _commandRunner = commandRunner;
    }

    public async Task<TerminationResult> GracefulTerminateAsync(int pid, TimeSpan timeout)
    {
        if (pid <= 0)
        {
            return TerminationResult.Failed("Invalid process id.");
        }

        if (!ProcessHelper.IsProcessAlive(pid))
        {
            return TerminationResult.NotFound("Process not found.");
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(pid);
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        process.CloseMainWindow();
                    }
                    else
                    {
                        await _commandRunner.RunAsync("taskkill", $"/PID {pid} /T", TimeSpan.FromSeconds(3));
                    }
                }
                catch
                {
                    await _commandRunner.RunAsync("taskkill", $"/PID {pid} /T", TimeSpan.FromSeconds(3));
                }
            }
            else
            {
                await _commandRunner.RunAsync("kill", $"-TERM {pid}", TimeSpan.FromSeconds(3));
            }
        }
        catch (UnauthorizedAccessException)
        {
            return TerminationResult.AccessDenied("Access denied.");
        }
        catch (Exception ex)
        {
            return TerminationResult.Failed(ex.Message);
        }

        var exited = await ProcessHelper.WaitForExitAsync(pid, timeout);
        return exited
            ? TerminationResult.Success("Process terminated gracefully.")
            : TerminationResult.StillRunning("Process is still running.");
    }

    public async Task<TerminationResult> ForceTerminateAsync(int pid, TimeSpan timeout)
    {
        if (pid <= 0)
        {
            return TerminationResult.Failed("Invalid process id.");
        }

        if (!ProcessHelper.IsProcessAlive(pid))
        {
            return TerminationResult.NotFound("Process not found.");
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                await _commandRunner.RunAsync("taskkill", $"/PID {pid} /T /F", TimeSpan.FromSeconds(3));
            }
            else
            {
                await _commandRunner.RunAsync("kill", $"-KILL {pid}", TimeSpan.FromSeconds(3));
            }
        }
        catch (UnauthorizedAccessException)
        {
            return TerminationResult.AccessDenied("Access denied.");
        }
        catch (Exception ex)
        {
            return TerminationResult.Failed(ex.Message);
        }

        var exited = await ProcessHelper.WaitForExitAsync(pid, timeout);
        return exited
            ? TerminationResult.Success("Process terminated forcefully.")
            : TerminationResult.Failed("Process is still running after forceful termination.");
    }
}
