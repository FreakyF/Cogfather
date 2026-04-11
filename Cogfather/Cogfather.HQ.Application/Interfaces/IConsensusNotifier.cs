using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

public interface IConsensusNotifier
{
    Task NotifyAsync(ConsensusResult result, CancellationToken cancellationToken = default);
}