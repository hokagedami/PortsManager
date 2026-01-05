#nullable enable
namespace PortsManager.Core.Services;

public enum TerminationResultKind
{
    Success,
    NotFound,
    AccessDenied,
    StillRunning,
    Failed
}

public sealed class TerminationResult
{
    public required TerminationResultKind Kind { get; init; }
    public string? Message { get; init; }

    public static TerminationResult Success(string? message = null) => new()
    {
        Kind = TerminationResultKind.Success,
        Message = message
    };

    public static TerminationResult NotFound(string? message = null) => new()
    {
        Kind = TerminationResultKind.NotFound,
        Message = message
    };

    public static TerminationResult AccessDenied(string? message = null) => new()
    {
        Kind = TerminationResultKind.AccessDenied,
        Message = message
    };

    public static TerminationResult StillRunning(string? message = null) => new()
    {
        Kind = TerminationResultKind.StillRunning,
        Message = message
    };

    public static TerminationResult Failed(string? message = null) => new()
    {
        Kind = TerminationResultKind.Failed,
        Message = message
    };
}
