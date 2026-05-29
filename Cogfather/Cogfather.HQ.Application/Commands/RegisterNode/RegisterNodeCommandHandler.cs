using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Metrics;
using Cogfather.HQ.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cogfather.HQ.Application.Commands.RegisterNode;

public class RegisterNodeCommandHandler : IRequestHandler<RegisterNodeCommand>
{
    private readonly ILogger<RegisterNodeCommandHandler> _logger;
    private readonly INodeRepository _nodeRepository;

    public RegisterNodeCommandHandler(INodeRepository nodeRepository, ILogger<RegisterNodeCommandHandler> logger)
    {
        _nodeRepository = nodeRepository;
        _logger = logger;
    }

    public async Task Handle(RegisterNodeCommand request, CancellationToken cancellationToken)
    {
        var existing = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogDebug("Node {NodeId} re-registered (already known)", request.NodeId[..Math.Min(8, request.NodeId.Length)]);
            return;
        }

        var node = new NodeRegistration(request.NodeId, request.Address);
        await _nodeRepository.AddAsync(node, cancellationToken);
        CogfatherMetrics.NodeReputation.WithLabels(request.NodeId[..Math.Min(8, request.NodeId.Length)], request.Address).Set(node.ReputationScore);
        _logger.LogInformation("Node registered: {NodeId} at {Address}", request.NodeId[..Math.Min(8, request.NodeId.Length)], request.Address);
    }
}