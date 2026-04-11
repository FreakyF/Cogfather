namespace Cogfather.Contracts.Messages.Heartbeat;

/// <summary>
/// Periodic liveness signal from a Node to HQ.
/// HQ uses this to detect offline nodes and update NodeRegistration.LastSeenAt.
/// </summary>
public sealed record NodeHeartbeatMessage(
    Guid NodeId,
    string DisplayName,
    DateTimeOffset SentAt,
    /// <summary>Current inventory snapshot for observability.</summary>
    IReadOnlyDictionary<string, int> InventorySnapshot
);