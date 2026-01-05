#nullable enable
namespace PortsManager.Core.Services;

public interface IProcessTerminator
{
    Task<TerminationResult> GracefulTerminateAsync(int pid, TimeSpan timeout);
    Task<TerminationResult> ForceTerminateAsync(int pid, TimeSpan timeout);
}
