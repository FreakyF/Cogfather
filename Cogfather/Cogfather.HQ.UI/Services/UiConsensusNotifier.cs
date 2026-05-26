using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace Cogfather.HQ.UI.Services;

public sealed class UiConsensusNotifier : IConsensusNotifier
{
    private readonly ConsensusEventService _eventService;
    private readonly IHubContext<ConsensusHub> _hubContext;
    private readonly ILogger<UiConsensusNotifier> _logger;

    public UiConsensusNotifier(IHubContext<ConsensusHub> hubContext, ConsensusEventService eventService,
        ILogger<UiConsensusNotifier> logger)
    {
        _hubContext = hubContext;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task NotifyAsync(ConsensusResult result, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consensus reached for {RecipeId}: {Verdict}", result.RecipeId, result.Verdict);
        await _hubContext.Clients.All.SendAsync("ConsensusReached", result, cancellationToken);
        _eventService.Raise(result);
        _logger.LogInformation("In-process event raised, subscribers: {Count}",
            _eventService.SubscriberCount);
    }
}
