using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace Cogfather.HQ.Infrastructure.Services;

public class ConsensusHub : Hub
{
}

public class SignalRConsensusNotifier : IConsensusNotifier
{
    private readonly IHubContext<ConsensusHub> _hubContext;

    public SignalRConsensusNotifier(IHubContext<ConsensusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(ConsensusResult result, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("ConsensusReached", result, cancellationToken);
    }
}