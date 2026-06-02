using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Enums;
using MediatR;

namespace Cogfather.HQ.Application.Commands.ClearNodeFault;

public class ClearNodeFaultCommandHandler : IRequestHandler<ClearNodeFaultCommand>
{
    private readonly INodeRepository _nodeRepository;

    public ClearNodeFaultCommandHandler(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task Handle(ClearNodeFaultCommand request, CancellationToken cancellationToken)
    {
        var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken)
                   ?? throw new InvalidOperationException($"Node '{request.NodeId}' not found.");

        node.SetFaultMode(FaultMode.None);
        await _nodeRepository.UpdateAsync(node, cancellationToken);
    }
}