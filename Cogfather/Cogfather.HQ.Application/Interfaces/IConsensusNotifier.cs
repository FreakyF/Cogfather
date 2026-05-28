using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Broadcasts consensus results to connected clients (SignalR hub and in-process event bus).
/// </summary>
public interface IConsensusNotifier
{
    /// <summary>Publishes <paramref name="result"/> to all active subscribers.</summary>
    Task NotifyAsync(ConsensusResult result, CancellationToken cancellationToken = default);
}
