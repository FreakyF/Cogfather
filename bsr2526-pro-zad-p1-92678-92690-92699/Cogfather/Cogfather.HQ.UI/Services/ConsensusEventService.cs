using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.UI.Services;

public sealed class ConsensusEventService
{
    public event Action<ConsensusResult>? OnConsensusReached;

    public int SubscriberCount => OnConsensusReached?.GetInvocationList().Length ?? 0;

    public void Raise(ConsensusResult result) => OnConsensusReached?.Invoke(result);
}
