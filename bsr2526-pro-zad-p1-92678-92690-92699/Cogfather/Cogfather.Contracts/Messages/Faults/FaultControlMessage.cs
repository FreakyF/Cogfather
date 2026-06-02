using Cogfather.Contracts.Enums;

namespace Cogfather.Contracts.Messages.Faults;

/// <summary>
///     Sent by HQ to a specific Node to activate or clear a fault injection mode.
///     Published to cogfather.faults exchange with routing key = NodeId.
/// </summary>
public sealed record FaultControlMessage(
    Guid NodeId,
    FaultModeContract FaultMode,
    int DelaySeconds
);