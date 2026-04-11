using Cogfather.HQ.Application.Interfaces;
using MediatR;

namespace Cogfather.HQ.Application.Commands.SetNodeFault;

public class SetNodeFaultCommandHandler : IRequestHandler<SetNodeFaultCommand>
{
    private readonly INodeRepository _nodeRepository;

    public SetNodeFaultCommandHandler(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task Handle(SetNodeFaultCommand request, CancellationToken cancellationToken)
    {
        var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken)
                   ?? throw new InvalidOperationException($"Node '{request.NodeId}' not found.");

        node.SetFaultMode(request.FaultMode);
        await _nodeRepository.UpdateAsync(node, cancellationToken);
    }
}