using Cogfather.HQ.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cogfather.HQ.Application.Commands.SetNodeFault;

public class SetNodeFaultCommandHandler : IRequestHandler<SetNodeFaultCommand>
{
    private readonly ILogger<SetNodeFaultCommandHandler> _logger;
    private readonly INodeRepository _nodeRepository;

    public SetNodeFaultCommandHandler(INodeRepository nodeRepository, ILogger<SetNodeFaultCommandHandler> logger)
    {
        _nodeRepository = nodeRepository;
        _logger = logger;
    }

    public async Task Handle(SetNodeFaultCommand request, CancellationToken cancellationToken)
    {
        var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken)
                   ?? throw new InvalidOperationException($"Node '{request.NodeId}' not found.");

        node.SetFaultMode(request.FaultMode);
        await _nodeRepository.UpdateAsync(node, cancellationToken);

        _logger.LogWarning("Fault mode set: node={NodeId} mode={FaultMode}",
            request.NodeId[..Math.Min(8, request.NodeId.Length)], request.FaultMode);
    }
}